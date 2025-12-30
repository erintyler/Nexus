# Nexus Design System Components

This document provides an overview of all the reusable Blazor components created from the Nexus design system.

## 📦 Components Created

### Button Components

#### NexusButton
A modern, gradient-based button component with multiple variants and states.

**Features:**
- Variants: Primary, Secondary, Success, Danger, Accent, Warning
- Sizes: Small, Medium, Large
- States: Normal, Disabled, Loading
- Smooth transitions and hover effects

**Usage:**
```razor
<NexusButton Variant="NexusButton.ButtonVariant.Primary" OnClick="HandleClick">
    Click Me
</NexusButton>

<NexusButton IsLoading="true" LoadingText="Processing...">
    Submit
</NexusButton>
```

#### NexusIconButton
Icon-only button for actions with minimal visual footprint.

**Features:**
- Colors: Primary, Secondary, Success, Danger, Warning, Info
- Sizes: Small, Medium, Large
- Accessible with aria-label support

**Usage:**
```razor
<NexusIconButton Color="NexusIconButton.IconButtonColor.Danger" AriaLabel="Delete">
    <svg>...</svg>
</NexusIconButton>
```

---

### Form Components

#### NexusTextInput
Modern text input with glass morphism effects and validation support.

**Features:**
- Label support with required indicator
- Error message display
- Helper text
- Custom types (text, email, password, etc.)
- Two-way binding

**Usage:**
```razor
<NexusTextInput 
    Label="Email Address"
    @bind-Value="email"
    IsRequired="true"
    Type="email"
    ErrorMessage="@errorMessage"
    HelperText="We'll never share your email" />
```

#### NexusTextArea
Multi-line text input component.

**Features:**
- Configurable rows
- Same validation and styling as NexusTextInput
- Auto-resizing disabled for consistent UI

**Usage:**
```razor
<NexusTextArea 
    Label="Description"
    @bind-Value="description"
    Rows="6"
    Placeholder="Enter description..." />
```

#### NexusLabel
Form label component with required indicator.

**Features:**
- Consistent styling across forms
- Required field indicator
- Associates with input via `for` attribute

**Usage:**
```razor
<NexusLabel For="username-input" IsRequired="true">
    Username
</NexusLabel>
```

---

### Display Components

#### NexusCard
Modern card component with three variants.

**Features:**
- Variants: Elevated (glass morphism), Interactive (hover effects), Bordered
- Gradient overlay for interactive cards
- Optional arrow icon
- Click handler support

**Usage:**
```razor
<NexusCard Variant="NexusCard.CardVariant.Interactive" OnClick="HandleCardClick">
    <h3>Card Title</h3>
    <p>Card content goes here</p>
</NexusCard>
```

#### NexusAlert
Alert/notification component with multiple types.

**Features:**
- Types: Success, Error, Warning, Info
- Optional title
- Optional icon
- Dismissible option
- Gradient backgrounds matching type

**Usage:**
```razor
<NexusAlert Type="NexusAlert.AlertType.Success" Title="Success!" Dismissible="true">
    Your changes have been saved.
</NexusAlert>
```

#### NexusBadge
Tag/badge component for labels and categories.

**Features:**
- Colors: Primary, Success, Danger, Accent, Warning, Info
- Styles: Gradient, Soft
- Optional label prefix
- Removable option
- Click handler for removal

**Usage:**
```razor
<NexusBadge 
    Label="Category" 
    Color="NexusBadge.BadgeColor.Primary" 
    Removable="true"
    OnRemove="HandleRemove">
    Design
</NexusBadge>
```

---

### Loading Components

#### NexusSpinner
Animated loading spinner component.

**Features:**
- Sizes: Small, Medium, Large, ExtraLarge
- Colors: Primary, Secondary, Success, Danger, Warning, Info, White
- CSS-based animation

**Usage:**
```razor
<NexusSpinner Size="NexusSpinner.SpinnerSize.Large" Color="NexusSpinner.SpinnerColor.Primary" />
```

#### NexusSkeleton
Skeleton loader for content placeholders.

**Features:**
- Shapes: Rectangle, Circle, Text
- Customizable width and height using Tailwind classes
- Pulse animation

**Usage:**
```razor
<NexusSkeleton Width="w-full" Height="h-4" />
<NexusSkeleton Shape="NexusSkeleton.SkeletonShape.Circle" Width="w-12" Height="h-12" />
```

---

## 🧪 Testing

All components have comprehensive unit tests using xUnit v3 and BUnit.

**Test Statistics:**
- Total Tests: 116
- All Passing ✅
- Coverage: All component variants, properties, and interactions

**Running Tests:**
```bash
cd Nexus.Frontend.UnitTests
dotnet test
```

---

## 🎨 Design Principles

All components follow these design principles:

1. **Vibrant Gradients** - Beautiful color transitions
2. **Glass Morphism** - Backdrop blur and transparency effects
3. **Smooth Animations** - Transform and transition effects
4. **Accessibility** - ARIA labels, keyboard navigation, semantic HTML
5. **Consistency** - Unified spacing, typography, and color palette

---

## 📚 Component Showcase

View all components in action at `/components` route.

The showcase page demonstrates:
- All button variants and states
- Form inputs with different configurations
- Card types
- Alert types
- Badge styles
- Loading states

---

## 🛠️ Development Guidelines

### Adding New Components

1. Create the component in `Nexus.Frontend.Client/Components/DesignSystem/`
2. Follow naming convention: `Nexus{ComponentName}.razor`
3. Use enums for variants/types
4. Support `AdditionalAttributes` for extensibility
5. Add XML documentation for all parameters

### Testing New Components

1. Create test file in `Nexus.Frontend.UnitTests/Components/DesignSystem/`
2. Inherit from `Bunit.TestContext`
3. Test all variants, states, and interactions
4. Use descriptive test names

### Code Formatting

Always run `dotnet format` before committing:
```bash
cd /home/runner/work/Nexus/Nexus
dotnet format
```

---

## 📄 License

Part of the Nexus project. All components follow the project's license.
