---
name: Nill Wrap-Up
description: A private Nill year-in-review presented as a photocopied scene zine.
colors:
  page-charcoal: "#1B1B1B"
  sheet-charcoal: "#2A2A2A"
  deep-charcoal: "#151515"
  ink-light: "#DCE0DF"
  muted-teal-gray: "#B2CBC5"
  nill-teal: "#4C6D6D"
  rule-teal: "rgba(178, 203, 197, 0.34)"
  white: "#fff"
  teal-hover: "#5c7f7f"
typography:
  display:
    fontFamily: '"Barlow Condensed", "Arial Narrow", sans-serif'
    fontSize: "clamp(4rem, 8vw, 6rem)"
    fontWeight: 700
    lineHeight: 0.88
    letterSpacing: "-0.035em"
  body:
    fontFamily: "Verdana, Arial, sans-serif"
    fontSize: "1rem"
    fontWeight: 400
    lineHeight: 1.65
spacing:
  "8": "0.5rem"
  "16": "1rem"
  "24": "1.5rem"
  "32": "2rem"
components:
  action-link:
    backgroundColor: "{colors.nill-teal}"
    textColor: "{colors.white}"
    typography: "{typography.body}"
    padding: "0.75rem 1.15rem"
  action-link-hover:
    backgroundColor: "{colors.teal-hover}"
  navigation-link:
    textColor: "{colors.muted-teal-gray}"
    typography: "{typography.body}"
---

# Design System: Nill Wrap-Up

## Overview

**Creative North Star: "Nill Scene Zine"**

The public wrap-up reads as one dark editorial sheet: charcoal stock, restrained photocopy grain, light ink, and muted teal details. Display headlines use self-hosted Barlow Condensed; Verdana-style sans text keeps explanations and navigation familiar and readable. The homepage's five-stat index is deliberately uneven and editorial rather than a row of cards.

The build is tonal and flat, using thin rules and changes in charcoal value instead of elevation. Actions are square, high-contrast links. The Barlow Condensed font file is bundled with its SIL Open Font License.

**Key Characteristics:**
- Continuous charcoal sheet with subtle repeating photocopy grain.
- Condensed uppercase display paired with readable Verdana-style body text.
- Muted teal marks links, rules, and the primary action; the CTA remains square.
- Editorial lists and verification rows use rules and alignment, not card containers.

## Colors

The palette stays within Nill's verified charcoal, gray-green, and desaturated teal, with white reserved for strong contrast.

### Primary
- **Nill Teal** (`{colors.nill-teal}`): The primary registration action and selected interface accents.

### Neutral
- **Page Charcoal** (`{colors.page-charcoal}`): The page and surrounding canvas.
- **Sheet Charcoal** (`{colors.sheet-charcoal}`): The editorial homepage sheet beneath the grain texture.
- **Deep Charcoal** (`{colors.deep-charcoal}`): Header, footer, and skip-link surfaces.
- **Light Ink** (`{colors.ink-light}`): Main headings and body text.
- **Muted Teal-Gray** (`{colors.muted-teal-gray}`): Supporting copy, links, and the masthead divider.
- **Translucent Teal Rule** (`{colors.rule-teal}`): Fine separators and sheet edges.
- **White** (`{colors.white}`): High-contrast action-link text and link hover.
- **Teal Hover** (`{colors.teal-hover}`): The action-link hover surface.

### Named Rules
**The Charcoal Stock Rule.** Keep the page in the verified charcoal range; use muted teal and light ink as accents rather than introducing unrelated hues.

## Typography

**Display Font:** Barlow Condensed (self-hosted; with Arial Narrow, sans-serif fallbacks)  
**Body Font:** Verdana (with Arial, sans-serif fallbacks)

**Character:** Barlow Condensed supplies the compact, poster-like editorial voice. Verdana's familiar shapes and generous leading make the explanatory copy easy to read against the dark sheet.

### Hierarchy
- **Display** (700, `clamp(4rem, 8vw, 6rem)`, 0.88 line-height, -0.035em tracking): The large uppercase homepage headline.
- **Headline** (700, `clamp(1.75rem, 3vw, 2.4rem)`, 1 line-height, -0.02em tracking): Section and index headings.
- **Title** (700, 1.3rem, 1.15 line-height): Compact labels in the statistics index and verification steps.
- **Step heading** (700, 1.5rem, 1.1 line-height): Verification-step titles.
- **Body** (400, 1rem, 1.65 line-height): Default explanatory text; supporting paragraphs use a measured maximum line length where specified.
- **Supporting copy** (400, `clamp(1.05rem, 1.7vw, 1.25rem)`): The lead sentence under the hero headline.

## Layout

The content sits in a centered column capped at 1200px. Header, main, and footer align to this width; the homepage sheet adds responsive horizontal padding, thin side rules, and the repeating 180px grain tile. The hero uses an asymmetric two-column grid (1.3fr / 0.7fr, with a 300px minimum for the index) and its sections use similarly editorial, unequal columns.

At widths of 800px and below, the hero, verification explanation, and privacy note stack into one column. At 560px and below, outer alignment narrows to 20px gutters, the masthead and footer become 68px minimum-height rows, and verification descriptions move below their step headings. The hero headline scales down for narrow screens. Repeated spacing values form a loose rhythm; rules and alignment do more structural work than boxed surfaces.

## Elevation & Depth

There are no box shadows. Depth comes from the deep-charcoal header and footer against the page, the slightly lighter sheet, the low-opacity photocopy grain, and thin translucent rules. The grain is a monochrome fractal-noise SVG, repeated at its native 180px square tile size.

## Shapes

The visual language is square and print-like: sheet edges and list divisions are straight rules, and the CTA has no rounded corners. The global keyboard-focus outline is the only rounded treatment (2px radius); it is an accessibility indicator, not a component shape.

## Components

### Actions
- **Character:** A restrained, square anchor with unmistakable contrast.
- **Primary:** Muted-teal background, white bold body text, 52px minimum height, and `0.75rem 1.15rem` padding.
- **Hover / Focus:** Hover shifts to the lighter teal; keyboard focus uses a 3px muted-teal-gray outline with a 5px offset.
- **Text action:** The in-page “How it works” link remains an underlined text link rather than a second filled button.
- **Route status:** `/Account/Login` uses cookie authentication; `/WrapUp` is protected and currently a not-ready placeholder; `/Account/Register` remains future, unimplemented work.

### Navigation
- **Masthead:** Deep-charcoal band with a thin lower rule; the NILL / WRAP-UP wordmark uses Barlow Condensed, bold weight, and slight tracking.
- **Links:** Muted teal-gray, underlined, and kept as text navigation; the header aligns brand and sign-in across the centered container.
- **Responsive:** The masthead keeps its horizontal brand/navigation arrangement at the documented mobile breakpoint.

### Editorial Index and Verification Steps
- **Statistics index:** A definition list with five label-and-description entries, separated by horizontal rules. It is not presented as a card grid and contains no fabricated totals.
- **Verification steps:** A semantic ordered list, numbered with a leading zero and set in an uneven three-column row on wide screens; descriptions shift beneath headings on narrow screens.
- **Privacy note:** A two-column, rule-bounded band that stacks at the tablet breakpoint.

## Do's and Don'ts

### Do:
- **Do** use the bundled Barlow Condensed face for display and the Verdana-style stack for reading text.
- **Do** keep primary actions square, high-contrast links with a visible keyboard-focus outline.
- **Do** use charcoal layering, grain, and fine rules to create print depth without elevation.
- **Do** keep personal recap content private and derived from the member's own Nill data.

### Don't:
- **Don't** invent activity values, rankings, comparisons, or another member's data.
- **Don't** turn the editorial index into a row of equal cards or add unsupported visual colors.
