"""Clean duplicate reports and sort proofs into week folders with notes."""
from pathlib import Path
from shutil import copy2, rmtree

ROOT = Path(r"C:\projects\clz\oraEarn\(s395172) Dharma_Bahadur_Budhathoki")
FIGS = ROOT / "figures"
TOOLS = ROOT / "_tools"

DUPLICATES = [
    "Assessment-2-GROUP4-COMPLETE.docx",
    "Assessment-2-GROUP4-COMPLETE.md",
    "Assessment-2-Project-Progress-Report.docx",
    "Assessment-2-Project-Progress-Report.md",
    "PRT681_Assessment2_Group4_OraEarn_Progress_Report.docx",
    "Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-8.xlsx",
    "Individual Timesheets - Dharma Bahadur budhathoki(S395172).xlsx",
]

SCRIPTS = [
    "_build_group4.py",
    "_decode_png.py",
    "_decode_shot.py",
    "_extra_proofs.py",
    "_feature_glimpses.py",
    "_make_diagrams.py",
    "_suraj_pack.py",
    "_update_week10.py",
    "_organise_weeks.py",
]

WEEKS = {
    "week2": {
        "note": """Week 2 — Dharma Bahadur Budhathoki (S395172)
Project start. I wrote the first OraEarn proposal so the group had one idea: hours, pay, expenses, savings, visa hours.

Proof in this folder
- auraEarn-project_proposal.docx

What I see
- One product name, not four different apps.
""",
        "files": [],
    },
    "week3": {
        "note": """Week 3 — Dharma Bahadur Budhathoki (S395172)
Problem statement, canvas and solution pictures. This is what we showed in class before any code.

Proof in this folder
- oraEarn/oraEarnProblemStaement.png
- oraEarn/Oraearn_project_canvas.png
- oraEarn/oraEarn_solution.png
""",
        "files": [],
    },
    "week4": {
        "note": """Week 4 — Dharma Bahadur Budhathoki (S395172)
5.5 hours on the timesheet. I picked OraEarn, listed jobs/shifts/expenses/savings, and started looking at ASP.NET packages.

Proof in this folder
- screenshots/how_to-use-.ASP.NET.nuget.png  (what I saw while adding NuGet packages)
- screenshots/rest-api.png
- figure-packages.png
- figure-folder-structure.png

Note
- Week 4 hours are 5.5. Cap is 1–6.
""",
        "files": [
            "figure-packages.png",
            "figure-folder-structure.png",
        ],
    },
    "week5": {
        "note": """Week 5 — Dharma Bahadur Budhathoki (S395172)
6 hours. Scrum Master work: Git branch app-s395172, ERD talk with Krishna, first backend/frontend setup.

Proof in this folder
- screenshorts/FinalErdAfterNormalization.png
- screenshorts/ResearchOnmodularMonolithicArchitecture.png
- screenshorts/setupofbackend.png
- figure-diagram-team-erd.png
- figure-krishna-erd.png
- figure-diagram-architecture.png

Note
- I did not draw Krishna’s ERD. I sat in the talk and then built the models to the same names.
""",
        "files": [
            "figure-diagram-team-erd.png",
            "figure-krishna-erd.png",
            "figure-diagram-architecture.png",
        ],
    },
    "week6": {
        "note": """Week 6 — Dharma Bahadur Budhathoki (S395172)
6 hours. Postgres + EF, packages, models from the ERD, Aura backend start. Individual meeting used as a sprint review.

Proof in this folder
- Individual Meeting Log - Week 06
- figure-backend-db.png
- figure-backend-program.png
- figure-packages.png

What I see
- API starting on :5203, database context, no Docker.
""",
        "files": [
            "figure-backend-db.png",
            "figure-backend-program.png",
            "figure-packages.png",
        ],
        "move": [
            ROOT / "Individual Meeting Log - Week 06 - Dharma Bahadur Budhathoki (S395172).docx",
        ],
    },
    "week7": {
        "note": """Week 7 — Dharma Bahadur Budhathoki (S395172)
6 hours. Login / register JWT, admin approval, expense CRUD, Swagger while building.

Proof in this folder
- Individual Meeting Log - Week 07
- figure-backend-auth.png
- figure-frontend-login-code.png
- figure-03-login-password-eye.png
- figure-login-desktop.png
- figure-expenses-desktop.png
- figure-expenses-add-desktop.png

What I see
- Login page with password eye. Expense list and add pop-out.
""",
        "files": [
            "figure-backend-auth.png",
            "figure-frontend-login-code.png",
            "figure-03-login-password-eye.png",
            "figure-login-desktop.png",
            "figure-04-register.png",
            "figure-register-visa-desktop.png",
            "figure-expenses-desktop.png",
            "figure-expenses-add-desktop.png",
            "figure-09-expense-list.png",
        ],
        "move": [
            ROOT / "Individual Meeting Log - Week 07 - Dharma Bahadur Budhathoki (S395172).docx",
        ],
    },
    "week8": {
        "note": """Week 8 — Dharma Bahadur Budhathoki (S395172)
6 hours. Main feature: jobs + shift calendar, visa 48-hour bar, dashboard Excel/PDF, Aura widget start.

Proof in this folder
- figure-jobs-desktop.png / mobile / add pop-out
- figure-shifts-desktop.png / mobile / add pop-out
- figure-dashboard-desktop.png / mobile
- figure-savings-desktop.png / add
- glimpse-seeder.png, glimpse-shift-backend.png, glimpse-visa-backend.png

What I see
- Black and white UI. Calendar, visa 28/48, money cards. This is the screen I spend the most time on.
""",
        "files": [
            "figure-jobs-desktop.png",
            "figure-jobs-add-desktop.png",
            "figure-jobs-mobile.png",
            "figure-shifts-desktop.png",
            "figure-shifts-add-desktop.png",
            "figure-shifts-mobile.png",
            "figure-shifts-ui.png",
            "figure-dashboard-desktop.png",
            "figure-dashboard-mobile.png",
            "figure-savings-desktop.png",
            "figure-savings-add-desktop.png",
            "figure-backend-shifts.png",
            "figure-backend-visa.png",
            "glimpse-seeder.png",
            "glimpse-shift-backend.png",
            "glimpse-visa-backend.png",
        ],
    },
    "week9": {
        "note": """Week 9 — Dharma Bahadur Budhathoki (S395172)
6 hours. Assessment 2 pack: diagrams, live GitHub shot, Jira board, Swagger fix and tests.

Proof in this folder
- figure-github-repo-live.png  (private repo, branch app-s395172, 50 commits)
- figure-jira-earn-board.png
- figure-swagger-api.png and figure-swagger-health.png  (GET /health = 200)
- figure-swagger-aura.png
- UML / architecture / sequence / Scrum Master diagrams

What I see
- Swagger UI on :5203. Aura docs on :8000. GitHub logged-in tree.
- No demo passwords in these pictures.
""",
        "files": [
            "figure-github-repo-live.png",
            "figure-git-branch-commits.png",
            "figure-jira-earn-board.png",
            "figure-jira-keys.png",
            "figure-swagger-api.png",
            "figure-swagger-health.png",
            "figure-swagger-aura.png",
            "figure-swagger-aura-health.png",
            "figure-api-live.png",
            "figure-aura-docs.png",
            "figure-diagram-swagger-tests.png",
            "figure-diagram-scrum-dharma.png",
            "figure-diagram-usecase.png",
            "figure-diagram-sequence-shift.png",
            "figure-diagram-uml-classes.png",
            "figure-jenish-uml-di.png",
            "figure-jenish-uml-mapping.png",
            "figure-jenish-uml-security.png",
        ],
    },
    "week10": {
        "note": """Week 10 — Dharma Bahadur Budhathoki (S395172)
6 hours. Final Learnline pack under my name. Timesheet now weeks 4–10 (41.5 hours).

Proof in this folder
- PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_OraEarn_Progress_Report.docx
- Individual Timesheets ... weeks 4-10.xlsx
- figure-timesheet-dharma.png
- figure-github-repo-live.png

What I see
- One report, one timesheet, my student ID on the file names.
- Upload zip is in Downloads: PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_Learnline.zip

Note
- Clone app-s395172, not main. Invite the lecturer to the private GitHub repo.
""",
        "files": [
            "figure-timesheet-dharma.png",
            "figure-github-repo-live.png",
        ],
        "extra": [],
    },
}

ROOT_NOTE = """Dharma Bahadur Budhathoki (S395172) — folder map

This folder used to have four copies of the same report and three timesheets. Those extras are gone.

Keep at the top (for Learnline)
- PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_OraEarn_Progress_Report.docx
- Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-10.xlsx

Weekly proof (open the week folder)
- week2 … week10 each have NOTES.txt and the screenshots I actually used that week.

Also here
- figures\\     all pictures the Word report embeds (do not delete)
- _tools\\      old Python builders (not for upload)
"""


def copy_fig(name: str, dest: Path):
    src = FIGS / name
    if src.exists():
        copy2(src, dest / name)
        return True
    print("missing figure", name)
    return False


def main():
    TOOLS.mkdir(exist_ok=True)
    removed = []
    for name in DUPLICATES:
        p = ROOT / name
        if p.exists():
            p.unlink()
            removed.append(name)
            print("removed", name)

    cache = ROOT / "__pycache__"
    if cache.exists():
        rmtree(cache)
        print("removed __pycache__")

    proof_pages = FIGS / "proof-pages"
    if proof_pages.exists():
        rmtree(proof_pages)
        print("removed figures/proof-pages")

    for name in SCRIPTS:
        p = ROOT / name
        if p.exists() and p.parent != TOOLS:
            target = TOOLS / name
            if target.exists():
                target.unlink()
            p.rename(target)
            print("moved", name, "-> _tools")

    for week, spec in WEEKS.items():
        folder = ROOT / week
        folder.mkdir(exist_ok=True)
        proof = folder / "proof"
        if spec["files"] or spec.get("extra") or spec.get("move"):
            proof.mkdir(exist_ok=True)
        for name in spec["files"]:
            copy_fig(name, proof)
        for src in spec.get("extra", []):
            if src.exists():
                copy2(src, proof / src.name)
            else:
                print("missing extra", src)
        for src in spec.get("move", []):
            if src.exists():
                dest = proof / src.name
                if dest.exists():
                    dest.unlink()
                src.rename(dest)
                print("moved", src.name, "->", week)
            else:
                print("missing move", src)
        (folder / "NOTES.txt").write_text(spec["note"].strip() + "\n", encoding="utf-8")
        print("wrote", week)

    (ROOT / "README_THIS_FOLDER.txt").write_text(ROOT_NOTE.strip() + "\n", encoding="utf-8")
    print("removed", len(removed), "duplicates")


if __name__ == "__main__":
    main()
