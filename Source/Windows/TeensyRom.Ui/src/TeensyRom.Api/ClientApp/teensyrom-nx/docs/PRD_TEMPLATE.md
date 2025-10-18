# [Feature Name] - Product Requirements Document

## Executive Summary 🎯

[Brief 2-3 sentence overview of what this feature is and why it matters. Focus on the business value and user impact, not technical implementation.]

## Product Overview 📋

### Core Capabilities

- **[Capability 1]**: [Brief description of what users can do]
- **[Capability 2]**: [Brief description of what users can do]
- **[Capability 3]**: [Brief description of what users can do]

### Target Use Cases

- **[Use Case 1]**: [Description of how this feature serves users]
- **[Use Case 2]**: [Description of how this feature serves users]
- **[Use Case 3]**: [Description of how this feature serves users]

---

## Core Concepts & Terminology 📚

### [Concept 1]

[Clear definition of the concept. Focus on WHAT it is, not HOW it works. Cross-reference related concepts using markdown links.]

### [Concept 2]

[Clear definition. Use sub-sections if needed to break down complex concepts:]

#### [Sub-concept A]

- **[Aspect]**: [Description]
- **[Aspect]**: [Description]

#### [Sub-concept B]

- **[Aspect]**: [Description]
- **[Aspect]**: [Description]

---

## User Personas & Scenarios 👥

### [Persona Name] [Emoji]

**Scenario**: [Describe the context and situation]
**Needs**: [List the key requirements this persona has]
**Workflow**: [Describe the high-level flow of how they use the feature]

### [Persona Name] [Emoji]

**Scenario**: [Describe the context and situation]
**Needs**: [List the key requirements this persona has]
**Workflow**: [Describe the high-level flow of how they use the feature]

---

## [Feature Area 1] ⏯️

### [Sub-feature Name]

[Description of what this sub-feature does for users]

#### [Behavior Name]

- **[Action]**: [What happens when user does this]
- **[Action]**: [What happens when user does this]
- **[Action]**: [What happens when user does this]

#### [Behavior Name]

[Describe the behavior in terms of user actions and system responses]

### [Sub-feature Name]

#### [Option/Mode Name]

[Describe what this option enables users to do:]

- **[Setting 1]**: [User-visible behavior]
- **[Setting 2]**: [User-visible behavior]
- **[Setting 3]**: [User-visible behavior]

---

## [Feature Area 2] 🗂️

### [Sub-feature System]

[Description of the system's purpose from a user perspective]

#### [Type/Mode Name]

[Describe what this type/mode does for users]

##### [Specific Variation]

- **[Aspect]**: [User-visible behavior]
- **[Aspect]**: [User-visible behavior]
- **[Aspect]**: [User-visible behavior]

##### [Specific Variation]

[Description of variation behavior]

#### [Behavior Name]

[Explain how different options interact or behave:]

- **[Scenario]**: [What happens in this case]
- **[Scenario]**: [What happens in this case]

---

## Business Rules & Constraints ⚖️

### [Rule Category 1]

- [Rule description focusing on WHAT must happen, not HOW]
- [Rule description]
- [Rule description]

### [Rule Category 2]

- [Rule description focusing on business logic and user-visible constraints]
- [Rule description]
- [Rule description]

### [Rule Category 3]

- [Rule description focusing on system boundaries and limitations]
- [Rule description]
- [Rule description]

---

## User Interface Requirements 🖥️

### Control Types

[Describe the types of interactions users will have, not the technical implementation]

#### [Control Type 1]

- **Behavior**: [How users interact with this control type]
- **Applications**: [Where these controls are used]
- **Visual Feedback**: [What users see when interacting]

#### [Control Type 2]

- **Behavior**: [How users interact with this control type]
- **Applications**: [Where these controls are used]
- **Visual Feedback**: [What users see when interacting]

### Display Rules

- **[Scenario/Context]**: [What users see in this situation]
- **[Scenario/Context]**: [What users see in this situation]
- **[Scenario/Context]**: [What users see in this situation]

### [UI Element Category]

- **[Element Type]**: [Purpose and user-visible information]
- **[Element Type]**: [Purpose and user-visible information]
- **[Element Type]**: [Purpose and user-visible information]

---

## Success Criteria ✅

### User Experience Goals

- **[Goal Category]**: [Specific measurable or observable outcome]
- **[Goal Category]**: [Specific measurable or observable outcome]
- **[Goal Category]**: [Specific measurable or observable outcome]
- **[Goal Category]**: [Specific measurable or observable outcome]

### Technical Performance

- **[Performance Category]**: [Observable system behavior requirement]
- **[Performance Category]**: [Observable system behavior requirement]
- **[Performance Category]**: [Observable system behavior requirement]

### Business Value

- **[Value Category]**: [How this creates business or user value]
- **[Value Category]**: [How this creates business or user value]
- **[Value Category]**: [How this creates business or user value]

---

## Integration Points 🔗

[Optional section: Describe how this feature relates to other areas of the application]

### [Related Area 1]

[Description of relationship and interaction points]

### [Related Area 2]

[Description of relationship and interaction points]

---

## Open Questions ❓

[Optional section: List any unresolved questions or areas needing clarification]

- **[Question]**: [Context and why this needs to be resolved]
- **[Question]**: [Context and why this needs to be resolved]

---

## Future Considerations 🔮

[Optional section: Ideas for future enhancements that are out of scope for initial release]

- **[Enhancement]**: [Brief description of potential future capability]
- **[Enhancement]**: [Brief description of potential future capability]

---

_This document describes the complete business requirements for [Feature Name]. All features must [any global constraints or integration requirements]._

---

## PRD Writing Guidelines 📝

### Core Principles

**Focus on WHAT, not HOW**
- ✅ "Users can adjust playback speed from -50% to +200%"
- ❌ "The SpeedControlService updates the speedSignal computed property"

**Use user-centric language**
- ✅ "The system automatically saves preferences"
- ❌ "The savePreferences() method is called on component destroy"

**Define concepts before using them**
- ✅ Create a "Core Concepts" section with clear definitions
- ❌ Use undefined terms throughout the document

**Cross-reference liberally**
- ✅ Use markdown links: [Shuffle Mode](#shuffle-mode)
- ❌ Assume readers remember earlier sections

### Document Structure

**Start broad, go specific**
- Executive Summary → Core Capabilities → Detailed Behaviors

**Group related features logically**
- Not by technical architecture, but by user mental models

**Use consistent section patterns**
- Capabilities → Behaviors → Rules → Success Criteria

### Writing Style

**Be precise about behavior**
- ✅ "Advances to next file when timer expires"
- ❌ "Moves to the next thing automatically"

**Define all modes and states**
- Every option, mode, or variant needs explicit definition

**Explain business rules clearly**
- Why certain behaviors happen in certain contexts

**Avoid technical jargon**
- Don't mention: components, services, signals, stores, APIs
- Do mention: views, areas, controls, features, behaviors

### What to Include

- User personas and scenarios
- Observable behaviors and interactions
- Business rules and constraints
- Success criteria (measurable outcomes)
- UI requirements (interaction patterns, not visual design)
- Integration points (how features connect)

### What to Exclude

- Implementation details
- Code snippets or technical architecture
- Specific component or service names
- Database schema or API endpoints
- Technical dependencies or frameworks
- How state is managed internally

### Quality Checklist

Before finalizing your PRD, ensure:

- [ ] All concepts are clearly defined in the terminology section
- [ ] Cross-references link to relevant concept definitions
- [ ] User personas represent actual use cases
- [ ] Business rules are complete and unambiguous
- [ ] Success criteria are measurable or observable
- [ ] No technical implementation details leaked through
- [ ] Document can be understood by non-technical stakeholders
- [ ] All features have clear user value propositions

---

## Example Transformations

### ❌ Technical Writing
"The PlayerStore manages playback state using NgRx signals. When the play() method is called, it dispatches an action that updates the isPlaying signal and calls the DeviceApiService to send commands to the hardware."

### ✅ PRD Writing
"The system maintains playback state for each device independently. When a user starts playback, the current file begins playing and the system updates the progress display. All playback commands are synchronized with the connected hardware device."

---

### ❌ Technical Writing
"Implement a computed signal that derives the effective speed from baseSpeed and activeModifiers using the following formula: effectiveSpeed = baseSpeed + sum(modifiers)"

### ✅ PRD Writing
"Speed adjustments combine additively. For example, a base speed of +10% with a +5% nudge results in +15% total playback speed. When the nudge is released, playback returns to the +10% base speed."
