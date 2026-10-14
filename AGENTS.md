# IdleGo repository rules

These rules apply to the whole repository.

## Required project context workflow

1. Before planning, editing code, editing Unity assets, or answering a question about the current architecture, read [PROJECT_PROGRESS.md](PROJECT_PROGRESS.md) completely.
2. Start from the files listed as the source of truth in that document. Do not scan the whole project unless the document is missing, stale, conflicts with the code, or the requested change crosses an undocumented system boundary.
3. Treat the current user request and current code as authoritative. If either conflicts with `PROJECT_PROGRESS.md`, follow the user/current code and correct the document in the same task.
4. After every project task, update `PROJECT_PROGRESS.md` before reporting completion:
   - update the affected system status and flow when behavior or ownership changed;
   - append a dated entry to `Recent changes` for material work;
   - add concrete unfinished work to `Open TODO` and remove TODOs that were completed;
   - record the checks actually run and do not claim Unity/Play Mode verification unless it was performed.
5. If work stops incomplete, document the exact remaining step and blocker in `Open TODO`. Never mark a feature complete only because code was added.

## Permanent implementation constraints

- New gameplay initialization uses `OnInit()` and cleanup uses `OnDespawn()`. Do not add `Awake()` to new gameplay classes.
- Avoid `GetComponent`, `FindObjectOfType`, `FindFirstObjectByType`, and similar scene searches. Use serialized references, managers, or explicit initialization.
- Do not add C# events unless the user explicitly requests them.
- Keep each C# file at or below 250 lines and each method at or below 45 lines. Split responsibilities into classes or focused partial files when needed.
- UI reads and requests mutations through `DataManager`. The owning data system mutates data, saves it, then tells registered views to refresh. UI does not directly mutate domain data and does not pass core data into refresh callbacks.
- Every pooled object derives from `GameUnit` and uses the existing `SimplePool` lifecycle.
- Use `TextMeshProUGUI` for game text.
- Add a short explanatory comment at important state transitions and non-obvious gameplay logic.
- Preserve user-authored Unity scene, prefab, and ScriptableObject changes. Inspect current assets before editing them.

