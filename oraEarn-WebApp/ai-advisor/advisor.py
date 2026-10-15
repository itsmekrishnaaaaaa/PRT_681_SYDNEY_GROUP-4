"""Aura — friendly, short replies. No suggestion chips."""

from models import AdvisorContext, ChatResponse, GoalContext


def _active(goals: list[GoalContext]) -> list[GoalContext]:
    """Goals that are not finished yet (Pending or InProgress)."""
    return [g for g in goals if g.status != "Completed"]


def _in_progress(goals: list[GoalContext]) -> list[GoalContext]:
    """Goals with some savings but not complete, used to suggest which goal to push next."""
    return [g for g in goals if g.status == "InProgress"]


def _full_name(ctx: AdvisorContext) -> str:
    """First and last name, or an empty string when neither is on the profile."""
    return f"{ctx.user.first_name} {ctx.user.last_name}".strip()


def _intent(message: str) -> str:
    """Pick a reply topic from keywords in the message; the first matching rule wins.

    Rules are plain substring checks, so order matters: specific questions (name, visa, hours worked) are
    tested before the short greetings and catch-all topics. Known limitation: short keywords also match
    inside other words, e.g. "ta" in "status" gives "thanks" and "hi" in "shift" or "this" gives "greet".
    """
    lower = message.lower().strip()

    if any(p in lower for p in ("my name", "what is my name", "what's my name", "who am i", "whats my name")):
        return "name"
    if any(p in lower for p in ("nationality", "where am i from", "my country")):
        return "nationality"
    if any(p in lower for p in ("over limit", "over my", "am i over", "breach")):
        return "over_limit"
    if any(p in lower for p in ("how many hours", "hours worked", "hours have i")):
        return "hours_worked"
    if any(p in lower for p in ("visa status", "my visa", "what visa", "what is my visa", "whats my visa", "working rights", "visa type")):
        return "visa_status"
    if any(w in lower for w in ("thanks", "thank you", "cheers", "ta")):
        return "thanks"
    if any(w in lower for w in ("bye", "goodbye", "see you", "later")):
        return "bye"
    if any(w in lower for w in ("hi", "hello", "hey", "g'day", "good morning")):
        return "greet"
    if any(w in lower for w in ("recommend", "what should", "advice", "suggest", "focus")):
        return "recommend"
    if any(w in lower for w in ("fix", "help", "how can i", "what do i do")):
        return "fix"
    if any(w in lower for w in ("goal", "saving", "how are my")):
        return "goals"
    if any(w in lower for w in ("money", "balance", "afford", "broke", "expense", "income")):
        return "money"
    if any(w in lower for w in ("hour", "work", "shift")):
        return "hours"
    if any(w in lower for w in ("how am i", "summary", "overview", "status")):
        return "general"
    if "visa" in lower or "limit" in lower:
        return "over_limit" if "over" in lower or "limit" in lower else "visa_status"
    return "general"


def _say(intent: str, ctx: AdvisorContext) -> str:
    """Build a short reply for the intent from the user, visa and analytics context.

    The visa figures (48-hour cap, remaining hours, near/over limit) are computed by the .NET
    VisaComplianceService and only read here. A few reply strings still say "48" literally, so update them
    too if the limit ever changes.
    """
    u = ctx.user
    v = ctx.visa
    a = ctx.analytics
    name = u.first_name or "there"

    if intent == "name":
        full = _full_name(ctx)
        return f"You're {full}." if full else "I don't have your name saved yet."

    if intent == "nationality":
        nat = u.nationality or "not listed"
        return f"Your nationality is {nat}."

    if intent == "visa_status":
        if not u.visa_status and not u.visa_type:
            return "I can't see visa details on your profile yet."
        label = f"{u.visa_type} ({u.visa_status})" if u.visa_type and u.visa_status else (u.visa_type or u.visa_status)
        if u.working_rights:
            return f"You're on a {label} — {u.working_rights}."
        return f"You're on a {label}."

    if intent == "hours_worked":
        if not v.applies:
            return "The 48-hour student cap doesn't apply to you."
        worked = round(v.fortnight_hours_worked, 1)
        if v.is_over_limit:
            over = round(worked - v.fortnight_hour_limit, 1)
            return f"You've worked {worked} hrs in the last 14 days — that's {over} hrs over the limit."
        return f"You've worked {worked} hrs in the last 14 days, with {v.remaining_hours:.1f} hrs left this fortnight."

    if intent == "over_limit":
        if not v.applies:
            return "You're all good — no student fortnight cap on your account."
        if v.is_over_limit:
            over = round(v.fortnight_hours_worked - v.fortnight_hour_limit, 1)
            return f"Yes, you're {over} hrs over the {v.fortnight_hour_limit:.0f}-hour limit right now."
        if v.is_near_limit:
            return f"Not yet — but you're close. Only {v.remaining_hours:.1f} hrs left this fortnight."
        return f"No, you're under the cap with {v.remaining_hours:.1f} hrs still available."

    if intent == "hours":
        if v.is_over_limit:
            return "I'd hold off on extra shifts until your hours drop back under the limit."
        if v.applies:
            return f"You could safely work up to {v.remaining_hours:.1f} more hrs this fortnight."
        return "There's no visa hour restriction on your account."

    if intent == "goals":
        goals = a.goals
        if not goals:
            return "You haven't set any savings goals yet."
        active = _active(goals)
        if not active:
            return "Nice — all your savings goals are complete."
        g = active[0]
        if len(active) > 1:
            if g.estimated_finish_date:
                return f"Top goal: {g.title} (${g.remaining_amount:,.0f} left, finish around {g.estimated_finish_date}). You have {len(active)} active goals."
            return f"Top goal: {g.title} (${g.remaining_amount:,.0f} left). You have {len(active)} active goals."
        if g.estimated_finish_date:
            return f"{g.title}: ${g.remaining_amount:,.0f} to go, likely done by {g.estimated_finish_date}."
        return f"{g.title}: ${g.remaining_amount:,.0f} still to save."

    if intent == "money":
        bal = a.net_balance
        if bal < 0:
            return f"You're down about ${abs(bal):,.0f} — spending is ahead of what you've earned."
        if bal > 0:
            return f"You're up about ${bal:,.0f} — earning more than you're spending."
        return "You're breaking even right now."

    if intent == "recommend":
        if v.is_over_limit:
            return "Ease off shifts for a bit and trim spending until you're back under 48 hrs."
        prog = _in_progress(a.goals)
        if prog:
            return f"Put energy into {prog[0].title} — ${prog[0].remaining_amount:,.0f} left to hit that goal."
        if a.net_balance < 0:
            return "Tighten expenses before taking on more work."
        return "Keep logging shifts and chipping away at your goals — you're on track."

    if intent == "fix":
        if v.is_over_limit:
            return "Cut or swap shifts in the next fortnight so your rolling hours fall below 48."
        if a.net_balance < 0:
            return "Open Expenses and cut one non-essential category for this month."
        return "Tell me what's going on — visa, hours, money, or a goal — and I'll help."

    if intent == "greet":
        return f"Hey {name}! I'm Aura. Ask me about your visa, hours, savings, or balance."

    if intent == "thanks":
        return "Any time, happy to help."

    if intent == "bye":
        return "See you later!"

    # general snapshot
    if v.applies and v.is_over_limit:
        return f"Heads up: you're over the visa hour cap. Balance is ${a.net_balance:,.0f}."
    if v.applies:
        return f"You've worked {v.fortnight_hours_worked:.0f} of {v.fortnight_hour_limit:.0f} hrs this fortnight. Balance: ${a.net_balance:,.0f}."
    return f"Balance: ${a.net_balance:,.0f}. Ask me about visa, hours, or goals anytime."


def chat(message: str, ctx: AdvisorContext) -> ChatResponse:
    """Entry point used by the endpoint. Suggestions stay empty but the field is kept for the API contract."""
    reply = _say(_intent(message), ctx)
    return ChatResponse(reply=reply, suggestions=[])
