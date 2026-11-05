# Copilot instructions

## C# style

- Keep exactly one type in each `.cs` file; do not nest DTOs or helper types inside another type.
- Use explicit constructors; do not use primary constructors.
- Use `var` only when the type is obvious from the expression, such as `new`, an explicit cast, or a literal. Otherwise, declare the explicit type.
- Write single-statement `if` bodies without braces. Use braces when the body has multiple statements.
- Extract intermediate values into local variables before passing them as method arguments instead of nesting calls inline.
- Keep classes small and focused. Prefer extracting a distinct responsibility into its own class over accumulating private helper methods in a service or controller.
- Prefer composition over inheritance; do not introduce abstract base classes just to share behavior.
- When changing constructor signatures, preserve the existing parameter indentation and line layout.

## C# indentation and layout

- Indent with 4 spaces per level; never use tabs.
- Put opening braces for types, methods, and control statements on their own line (Allman style), as in the existing sources.
- Never re-wrap a signature, parameter list, or argument list that already fits on a single line. Add, remove, or reorder members in place and keep the original single-line form; do not turn it into a multi-line layout (or the reverse) just because you touched it.
- When a signature or call genuinely must be wrapped, put one parameter or argument per line indented one level deeper, and keep the closing parenthesis on the same line as the last parameter.
- Indent continuation lines one level deeper than the statement they belong to.
- Do not reindent, realign, or reformat lines you are not otherwise changing; keep diffs free of whitespace-only churn.
