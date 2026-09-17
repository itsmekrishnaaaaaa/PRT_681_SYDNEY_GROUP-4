from pydantic import BaseModel, Field


class UserContext(BaseModel):
    first_name: str = ""
    last_name: str = ""
    email: str = ""
    nationality: str = ""
    visa_type: str = ""
    visa_status: str = ""
    working_rights: str = ""
    visa_expiry_date: str | None = None


class VisaContext(BaseModel):
    applies: bool = False
    visa_status: str = ""
    working_rights: str = ""
    fortnight_hours_worked: float = 0
    fortnight_hour_limit: float = 48
    remaining_hours: float = 0
    is_over_limit: bool = False
    is_near_limit: bool = False


class GoalContext(BaseModel):
    title: str
    status: str
    hours_needed: float = 0
    avg_hourly_rate: float = 0
    avg_hours_per_week: float = 0
    remaining_amount: float = 0
    estimated_finish_date: str | None = None
    weeks_to_complete: int | None = None


class NotificationContext(BaseModel):
    type: str
    message: str
    severity: str


class AnalyticsContext(BaseModel):
    net_balance: float = 0
    total_allocation_percent: float = 0
    monthly_income_estimate: float = 0
    goals: list[GoalContext] = Field(default_factory=list)
    notifications: list[NotificationContext] = Field(default_factory=list)


class AdvisorContext(BaseModel):
    user: UserContext = Field(default_factory=UserContext)
    visa: VisaContext = Field(default_factory=VisaContext)
    analytics: AnalyticsContext = Field(default_factory=AnalyticsContext)


class ChatRequest(BaseModel):
    message: str
    context: AdvisorContext


class ChatResponse(BaseModel):
    reply: str
    suggestions: list[str]
