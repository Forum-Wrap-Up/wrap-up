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
