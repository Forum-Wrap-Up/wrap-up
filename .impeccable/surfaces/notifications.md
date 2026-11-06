---
version: 1
slug: "notifications"
primary_target: "src/ForumWrapUp.Web/Views/Shared/_Notification.cshtml"
related_targets:
  - "src/ForumWrapUp.Web/wwwroot/js/notifications.js"
  - "src/ForumWrapUp.Web/Views/Shared/_Layout.cshtml"
---

# Reusable status notifications

## Scope and visitor mode

Operate. Non-critical action confirmations use a reusable notification. The logout message is the first caller; errors remain beside their form. Default duration is the user's chosen six seconds, configurable per notification; zero means manual dismissal only.

## Direction contract

THESIS: Confirm an action without permanently occupying or shifting the page.

OWN-WORLD: Inherit Nill charcoal, readable Verdana, thin teal rules and the external close SVG; no inline SVG, library or new visual identity.

STORY: Read the confirmation, continue browsing, or close it. Hover, keyboard focus and a hidden browser tab pause the remaining reading time.

FIRST VIEWPORT: A compact notification stack at bottom right with message and a 44px close control; narrow screens retain side gutters and wrap long text. Without JavaScript, retain an inline readable message with the nonfunctional close control hidden.

FORM: Code-led local extension of Nill Scene Zine, seed:22d52a57ef771531, inherited from the corroborated homepage contract. The signature behavior is quiet timed dismissal without content reflow.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
