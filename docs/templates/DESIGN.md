# Design

## Priority
1. User's explicit design instructions or reference
2. Existing design system or repo styles (match them; do not restyle)
3. This default

Apply the default only to a new web app with no design direction. For a B2B dashboard preset, use the default as written. Otherwise, keep the tokens/components/interaction rules and change layout/colors to fit the product.

## Default
Minimalist, professional, corporate. White surfaces on #F7F8FA, borders #E3E6EB, text #111827, muted text #5B6472, one accent #1F3A5F. Irregular brand colors are not allowed.

- Type: Inter, 400/500/600. Base 14px, page title 24px.
- Layout: left sidebar 232px, top bar, content max-width 1100px.
- Style: crisp 1px borders, 6-8px radii. Shadows only if needed: subtle and layered. No gradients, no glassmorphism, no glows, no emoji in UI. Use Lucide line icons sparingly.
- Spacing: 8px grid.

## Responsive

- Mobile <768px: single column, sidebar as hamburger overlay, hit targets >=44px, inputs >=16px, tables scroll horizontally.
- Tablet 768-1100px: sidebar collapses to icon-only 64px, nav stays visible, content scrolls.
- Desktop >1100px: 232px sidebar fixed, content max 1100px centered, dense tables visible.
- No horizontal page scroll except intentional data tables with sticky first column.
- Multi-column grids stack, forms go full-width, top bar condenses.

## UI Quality: follow https://vercel.com/design/guidelines

Interactions
- Everything works by keyboard. Visible :focus-visible ring on all focusables.
- Hit targets >=24px desktop, >=44px mobile. Mobile inputs >=16px font.
- Never block paste or zoom.
- Navigation uses <a>, never <button> or <div>.
- Confirm destructive actions or offer Undo.
- Keep state in the URL (filters, tabs, pagination).
- Loading buttons keep their label and show a spinner.

Forms
- Every control has a <label>.
- Don't block typing. Don't pre-disable submit.
- Show errors next to fields. Focus the first error on submit.
- Use the right type, inputmode, and autocomplete.
- Placeholders show an example value and end with "…".

Content
- Design every state: empty, loading, error, dense.
- Skeletons match the final layout.
- No dead ends: every screen offers a next step.
- Never use color alone for status. Add a text label.
- Use tabular-nums for number columns.
- Use "…" not "...", curly quotes, and a non-breaking space in "10 MB".
- Semantic HTML first, ARIA second. Proper heading order + skip link.
- Icon-only buttons get an aria-label.

Motion & polish
- Honor prefers-reduced-motion.
- Animate transform/opacity only. Never use transition: all.
- Child radius <= parent radius.
- Hover/active/focus states have more contrast than rest.
- Set color-scheme and theme-color.

## Tokens
- Define colors, spacing, radii, and font sizes as CSS variables in one file (tokens.css or Tailwind config).
- Never hardcode hex values or px sizes in components. Use tokens.
- Include dark mode tokens from the start (prefers-color-scheme).

## Components (build once, reuse everywhere)
Button (primary, secondary, danger), Input, Select, Checkbox, Table, Modal/Confirm, Toast, Badge, Empty state, Skeleton, Page header. Don't create one-off variants.

## Anti-Slop Rules
Never use:
- Gradients (backgrounds, buttons, text), glassmorphism, glows, blur blobs
- Emoji or sparkle icons in UI. Use a single consistent line-icon set, sparingly.
- Colored left-border cards, or cards for everything. Prefer dividers and spacing.
- Centered hero + 3 identical feature cards
- Fake stats, fake testimonials, lorem ipsum. Use clear placeholders.
- Marketing copy inside app screens ("Supercharge", "Welcome back!")
- Default framework colors (indigo-500 etc.) or one shadow style on everything

Always:
- One accent color, one typeface, one radius value, one spacing scale
- Build hierarchy with type size/weight and spacing, not boxes
- Dense, scannable layouts for work screens (tables, compact rows)
- Plain, specific, action-oriented copy ("Add Record", not "Get Started!")
- Design empty, error, loading, and long-content states
- Strict grid alignment; one button style + one secondary style
- Before finishing, review the UI and remove anything decorative that doesn't help the user do the task

## Before Finishing, Check
- Keyboard-only walkthrough works, focus ring visible
- Every screen has empty, loading, and error states
- No hardcoded colors; no console errors
- 360px width has no horizontal scroll
- Contrast passes; status isn't color-only
- Removed anything decorative that doesn't help the task
Report anything you couldn't check. Don't claim it passed.

## Risks
Rules fight each other. "Minimal diff" (touch only needed lines) applies to existing code. New projects get scaffolded fully.

Don't promise "never looks like AI." Rules reduce slop, but a human review of the first screen still matters. Treat the first output as a draft and give one round of feedback.

## Pages
