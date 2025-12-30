# Nexus Design System ✨

> **Modern. Sleek. Personality.** A vibrant design system that brings your application to life.

## Philosophy

This design system embraces:
- 🎨 **Vibrant Gradients** - Beautiful color transitions that catch the eye
- 💫 **Smooth Animations** - Delightful micro-interactions that feel responsive
- 🔮 **Glass Morphism** - Modern, layered designs with backdrop blur
- ⚡ **Performance** - Optimized transitions and transforms
- ♿ **Accessibility** - Focus states, semantic HTML, keyboard navigation

## Table of Contents
1. [Color Palette](#color-palette)
2. [Typography](#typography)
3. [Spacing](#spacing)
4. [Components](#components)
   - [Buttons](#buttons)
   - [Form Inputs](#form-inputs)
   - [Cards](#cards)
   - [Tags/Badges](#tagsbadges)
   - [Alerts/Messages](#alertsmessages)
   - [Loading States](#loading-states)
   - [File Upload](#file-upload)
5. [Layout](#layout)
6. [Best Practices](#best-practices)

---

## Color Palette

### Modern Gradient Palette
```css
/* Primary Gradients - Blue to Indigo */
from-blue-600 to-indigo-600    /* Primary actions */
from-blue-700 to-indigo-700    /* Primary hover */
from-blue-50 to-indigo-50      /* Light backgrounds */

/* Success Gradients - Emerald to Teal */
from-emerald-500 to-teal-600   /* Success actions */
from-emerald-600 to-teal-700   /* Success hover */
from-emerald-50 to-teal-50     /* Success backgrounds */

/* Danger Gradients - Red to Pink */
from-red-500 to-pink-600       /* Destructive actions */
from-red-600 to-pink-700       /* Danger hover */
from-red-50 to-pink-50         /* Error backgrounds */

/* Accent Gradients - Violet to Purple */
from-violet-500 to-purple-600  /* Accent actions */
from-violet-600 to-purple-700  /* Accent hover */
from-violet-50 to-purple-50    /* Accent backgrounds */

/* Warning Gradients - Amber to Orange */
from-amber-500 to-orange-600   /* Warning actions */
from-amber-50 to-yellow-50     /* Warning backgrounds */

/* Neutral Colors */
bg-gray-50   /* Input backgrounds, subtle backgrounds */
bg-gray-100  /* Disabled states */
bg-gray-200  /* Borders */
text-gray-500 /* Placeholder text */
text-gray-700 /* Secondary text */
text-gray-800 /* Primary text */
```

### Usage Guidelines
- **Primary Gradients (Blue-Indigo)**: Main actions, links, active states
- **Success Gradients (Emerald-Teal)**: Success states, confirmations, positive actions
- **Danger Gradients (Red-Pink)**: Errors, warnings, destructive actions
- **Accent Gradients (Violet-Purple)**: Special features, highlights, accent buttons
- **Gray**: Neutral elements, backgrounds, borders

---

## Typography

### Text Sizes
```html
<!-- Headings -->
<h1 class="text-2xl font-bold">Page Title (2xl)</h1>
<h2 class="text-xl font-bold">Section Title (xl)</h2>
<h3 class="text-lg font-semibold">Subsection (lg)</h3>

<!-- Body Text -->
<p class="text-base">Regular body text (base)</p>
<p class="text-sm">Small body text, labels (sm)</p>
<p class="text-xs">Helper text, captions (xs)</p>
```

### Font Weights
- `font-bold` (700) - Headings, important text
- `font-semibold` (600) - Subheadings, emphasis
- `font-medium` (500) - Buttons, labels
- `font-normal` (400) - Body text

---

## Spacing

### Standard Spacing Scale
```
p-2  = 0.5rem (8px)   - Tight padding
p-3  = 0.75rem (12px) - Small padding
p-4  = 1rem (16px)    - Default padding
p-6  = 1.5rem (24px)  - Medium padding
p-8  = 2rem (32px)    - Large padding

gap-2 = 0.5rem  - Tight gaps
gap-3 = 0.75rem - Default gaps
gap-4 = 1rem    - Medium gaps
gap-6 = 1.5rem  - Large gaps

mb-2, mb-3, mb-4, mb-6 - Bottom margins (same values)
```

### Layout Spacing
- **Form fields**: `mb-6` between fields
- **Buttons**: `gap-3` between button groups
- **Cards**: `p-6` for card padding
- **Container**: `p-6` for page container

---

## Components

### Buttons

#### Primary Button
```html
<button class="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg 
               hover:bg-blue-700 focus:outline-none focus:ring-2 
               focus:ring-blue-500 focus:ring-offset-2 transition-colors">
    Primary Action
</button>
```

#### Success Button
```html
<button class="px-6 py-3 bg-green-600 text-white font-medium rounded-lg 
               hover:bg-green-700 focus:outline-none focus:ring-2 
               focus:ring-green-500 focus:ring-offset-2 transition-colors">
    Save / Submit
</button>
```

#### Secondary Button
```html
<button class="px-6 py-3 bg-gray-200 text-gray-700 font-medium rounded-lg 
               hover:bg-gray-300 focus:outline-none focus:ring-2 
               focus:ring-gray-500 focus:ring-offset-2 transition-colors">
    Cancel / Secondary
</button>
```

#### Small Button
```html
<button class="px-4 py-2 bg-blue-600 text-white rounded-lg 
               hover:bg-blue-700 focus:outline-none focus:ring-2 
               focus:ring-blue-500 transition-colors">
    Small Action
</button>
```

#### Disabled State
```html
<button disabled class="px-6 py-3 bg-gray-400 text-white font-medium rounded-lg 
                        cursor-not-allowed opacity-50">
    Disabled Button
</button>
```

#### Icon Button
```html
<button class="p-2 text-blue-600 hover:text-blue-800 focus:outline-none">
    <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
        <!-- Icon path -->
    </svg>
</button>
```

---

### Form Inputs

#### Text Input
```html
<div class="mb-6">
    <label for="input-id" class="block text-sm font-medium text-gray-700 mb-2">
        Label Text
    </label>
    <input 
        type="text" 
        id="input-id"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg 
               focus:ring-2 focus:ring-blue-500 focus:border-transparent 
               outline-none transition-all"
        placeholder="Enter text..." />
</div>
```

#### Text Input with Validation Error
```html
<div class="mb-6">
    <label class="block text-sm font-medium text-gray-700 mb-2">
        Label Text <span class="text-red-500">*</span>
    </label>
    <input 
        type="text"
        class="w-full px-3 py-2 border border-red-500 rounded-lg 
               focus:ring-2 focus:ring-red-500 outline-none"
        placeholder="Enter text..." />
    <p class="text-red-600 text-sm mt-1">Error message here</p>
</div>
```

#### Text Input with Helper Text
```html
<div class="mb-6">
    <label class="block text-sm font-medium text-gray-700 mb-2">
        Label Text
    </label>
    <input 
        type="text"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg 
               focus:ring-2 focus:ring-blue-500 focus:border-transparent 
               outline-none transition-all"
        placeholder="Enter text..." />
    <p class="text-xs text-gray-500 mt-1">Helper text or character count</p>
</div>
```

#### Textarea
```html
<div class="mb-6">
    <label class="block text-sm font-medium text-gray-700 mb-2">
        Description
    </label>
    <textarea 
        rows="4"
        class="w-full px-3 py-2 border border-gray-300 rounded-lg 
               focus:ring-2 focus:ring-blue-500 focus:border-transparent 
               outline-none transition-all resize-none"
        placeholder="Enter description..."></textarea>
</div>
```

#### Select/Dropdown
```html
<div class="mb-6">
    <label class="block text-sm font-medium text-gray-700 mb-2">
        Select Option
    </label>
    <select class="w-full px-3 py-2 border border-gray-300 rounded-lg 
                   focus:ring-2 focus:ring-blue-500 focus:border-transparent 
                   outline-none transition-all">
        <option>Option 1</option>
        <option>Option 2</option>
    </select>
</div>
```

#### Checkbox
```html
<div class="flex items-center mb-4">
    <input 
        type="checkbox" 
        id="checkbox-id"
        class="w-4 h-4 text-blue-600 border-gray-300 rounded 
               focus:ring-2 focus:ring-blue-500" />
    <label for="checkbox-id" class="ml-2 text-sm text-gray-700">
        Checkbox label
    </label>
</div>
```

---

### Cards

#### Basic Card
```html
<div class="bg-white shadow-md rounded-lg p-6">
    <h2 class="text-xl font-bold text-gray-800 mb-4">Card Title</h2>
    <p class="text-gray-700">Card content goes here...</p>
</div>
```

#### Card with Border
```html
<div class="bg-white border border-gray-200 rounded-lg p-6">
    <h3 class="text-lg font-semibold text-gray-800 mb-2">Card Title</h3>
    <p class="text-gray-600 text-sm">Card content goes here...</p>
</div>
```

#### Hover Card
```html
<div class="bg-white shadow-md rounded-lg p-6 
            hover:shadow-lg transition-shadow cursor-pointer">
    <h3 class="text-lg font-semibold text-gray-800 mb-2">Hover Card</h3>
    <p class="text-gray-600 text-sm">Hover for effect...</p>
</div>
```

#### Image Card
```html
<div class="bg-white shadow-md rounded-lg overflow-hidden">
    <img src="image.jpg" alt="Description" class="w-full h-48 object-cover" />
    <div class="p-4">
        <h3 class="text-lg font-semibold text-gray-800 mb-2">Image Title</h3>
        <p class="text-gray-600 text-sm">Description text...</p>
    </div>
</div>
```

---

### Tags/Badges

#### Default Tag
```html
<span class="inline-flex items-center bg-blue-100 text-blue-800 text-sm 
             px-3 py-1 rounded-full">
    Tag Label
</span>
```

#### Tag with Icon/Close Button
```html
<div class="inline-flex items-center bg-blue-100 text-blue-800 text-sm 
            px-3 py-1 rounded-full">
    <span class="font-semibold">Type:</span>
    <span class="ml-1">Value</span>
    <button class="ml-2 text-blue-600 hover:text-blue-800 focus:outline-none">
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
        </svg>
    </button>
</div>
```

#### Color Variants
```html
<!-- Success -->
<span class="bg-green-100 text-green-800 text-sm px-3 py-1 rounded-full">
    Success Tag
</span>

<!-- Warning -->
<span class="bg-yellow-100 text-yellow-800 text-sm px-3 py-1 rounded-full">
    Warning Tag
</span>

<!-- Error -->
<span class="bg-red-100 text-red-800 text-sm px-3 py-1 rounded-full">
    Error Tag
</span>

<!-- Gray -->
<span class="bg-gray-100 text-gray-800 text-sm px-3 py-1 rounded-full">
    Neutral Tag
</span>
```

#### Tag Group
```html
<div class="flex flex-wrap gap-2">
    <span class="bg-blue-100 text-blue-800 text-sm px-3 py-1 rounded-full">Tag 1</span>
    <span class="bg-blue-100 text-blue-800 text-sm px-3 py-1 rounded-full">Tag 2</span>
    <span class="bg-blue-100 text-blue-800 text-sm px-3 py-1 rounded-full">Tag 3</span>
</div>
```

---

### Alerts/Messages

#### Success Alert
```html
<div class="p-4 rounded-lg bg-green-50 text-green-800 border border-green-200">
    <div class="flex items-start">
        <svg class="w-5 h-5 text-green-600 mt-0.5 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
        </svg>
        <p class="font-medium">Success! Your action was completed.</p>
    </div>
</div>
```

#### Error Alert
```html
<div class="p-4 rounded-lg bg-red-50 text-red-800 border border-red-200">
    <div class="flex items-start">
        <svg class="w-5 h-5 text-red-600 mt-0.5 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd"></path>
        </svg>
        <p class="font-medium">Error: Something went wrong.</p>
    </div>
</div>
```

#### Warning Alert
```html
<div class="p-4 rounded-lg bg-yellow-50 text-yellow-800 border border-yellow-200">
    <div class="flex items-start">
        <svg class="w-5 h-5 text-yellow-600 mt-0.5 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"></path>
        </svg>
        <p class="font-medium">Warning: Please review this information.</p>
    </div>
</div>
```

#### Info Alert
```html
<div class="p-4 rounded-lg bg-blue-50 text-blue-800 border border-blue-200">
    <div class="flex items-start">
        <svg class="w-5 h-5 text-blue-600 mt-0.5 mr-3" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"></path>
        </svg>
        <p class="font-medium">Info: Here's some helpful information.</p>
    </div>
</div>
```

#### Simple Message (no border)
```html
<div class="p-4 rounded-lg bg-green-50 text-green-800">
    Operation completed successfully!
</div>
```

---

### Loading States

#### Spinner
```html
<svg class="animate-spin h-5 w-5 text-blue-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
</svg>
```

#### Button with Loading
```html
<button disabled class="px-6 py-3 bg-blue-600 text-white font-medium rounded-lg cursor-not-allowed">
    <span class="flex items-center justify-center">
        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        Loading...
    </span>
</button>
```

#### Skeleton Loader
```html
<div class="animate-pulse">
    <div class="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
    <div class="h-4 bg-gray-200 rounded w-1/2"></div>
</div>
```

---

### File Upload

#### Drag and Drop Area (Empty)
```html
<label class="flex flex-col items-center justify-center w-full h-64 
              border-2 border-gray-300 border-dashed rounded-lg 
              cursor-pointer bg-gray-50 hover:bg-gray-100 transition-colors">
    <div class="flex flex-col items-center justify-center pt-5 pb-6">
        <svg class="w-10 h-10 mb-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                  d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"></path>
        </svg>
        <p class="mb-2 text-sm text-gray-500">
            <span class="font-semibold">Click to upload</span> or drag and drop
        </p>
        <p class="text-xs text-gray-500">PNG, JPG, GIF up to 10MB</p>
    </div>
    <input type="file" class="hidden" />
</label>
```

#### Drag and Drop Area (With File)
```html
<label class="flex flex-col items-center justify-center w-full h-64 
              border-2 border-green-300 border-dashed rounded-lg 
              cursor-pointer bg-green-50 hover:bg-green-100 transition-colors">
    <div class="flex flex-col items-center justify-center pt-5 pb-6">
        <svg class="w-10 h-10 mb-3 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
        </svg>
        <p class="mb-2 text-sm text-gray-700 font-semibold">image.jpg</p>
        <p class="text-xs text-gray-500">image/jpeg - 2.4 MB</p>
    </div>
    <input type="file" class="hidden" />
</label>
```

---

## Layout

### Page Container
```html
<div class="max-w-2xl mx-auto p-6">
    <!-- Content -->
</div>
```

### Wide Container
```html
<div class="max-w-6xl mx-auto p-6">
    <!-- Content -->
</div>
```

### Full Width Container
```html
<div class="w-full p-6">
    <!-- Content -->
</div>
```

### Two Column Layout
```html
<div class="grid grid-cols-1 md:grid-cols-2 gap-6">
    <div>Column 1</div>
    <div>Column 2</div>
</div>
```

### Three Column Layout
```html
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <div>Column 1</div>
    <div>Column 2</div>
    <div>Column 3</div>
</div>
```

### Flex Layout (Horizontal)
```html
<div class="flex gap-3">
    <div>Item 1</div>
    <div>Item 2</div>
</div>
```

### Flex Layout (Space Between)
```html
<div class="flex justify-between items-center">
    <div>Left</div>
    <div>Right</div>
</div>
```

---

## Best Practices

### 1. Consistent Spacing
- Use the standard spacing scale (mb-6 for form fields, gap-3 for buttons)
- Maintain consistent padding in cards and containers (p-6)
- Use consistent gaps in flex and grid layouts

### 2. Color Usage
- Stick to the defined color palette
- Use semantic colors (green for success, red for errors)
- Maintain sufficient contrast for accessibility (WCAG AA minimum)

### 3. Interactive States
Always include:
- `:hover` states for interactive elements
- `:focus` states with visible focus rings
- `:disabled` states that are visually distinct
- Smooth transitions between states (`transition-colors`, `transition-shadow`)

### 4. Accessibility
- Use semantic HTML elements
- Include proper labels for all form inputs
- Ensure sufficient color contrast
- Add ARIA labels where needed
- Make all interactive elements keyboard accessible

### 5. Responsive Design
- Use responsive utilities (`md:`, `lg:` prefixes)
- Test layouts on mobile, tablet, and desktop
- Use `flex-wrap` and `gap` for flexible layouts
- Consider mobile-first design approach

### 6. Form Design
- Always show labels for inputs
- Use `<span class="text-red-500">*</span>` for required fields
- Show validation errors inline below inputs
- Disable submit buttons during processing
- Provide clear feedback after form submission

### 7. Loading States
- Show loading indicators for async operations
- Disable buttons during processing
- Use skeleton loaders for content loading
- Provide progress feedback for long operations

### 8. Consistency
- Use consistent border radius (`rounded-lg` for most elements)
- Maintain consistent button sizes within the same context
- Use consistent icon sizes (w-5 h-5 for most icons)
- Follow the same pattern for similar components

---

## Quick Reference Class Combinations

### Standard Input
```
w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all
```

### Primary Button
```
px-6 py-3 bg-blue-600 text-white font-medium rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors
```

### Card
```
bg-white shadow-md rounded-lg p-6
```

### Tag
```
inline-flex items-center bg-blue-100 text-blue-800 text-sm px-3 py-1 rounded-full
```

### Alert
```
p-4 rounded-lg bg-green-50 text-green-800 border border-green-200
```

---

## Icons

For icons, we recommend using:
- **Heroicons** (https://heroicons.com/) - Tailwind-compatible SVG icons
- **Lucide** (https://lucide.dev/) - Beautiful, consistent icon set
- Keep icon sizes consistent: `w-5 h-5` for inline icons, `w-10 h-10` for larger decorative icons

### Common Icon Sizes
```html
<!-- Small icon (inline with text) -->
<svg class="w-4 h-4">...</svg>

<!-- Default icon -->
<svg class="w-5 h-5">...</svg>

<!-- Large icon (decorative) -->
<svg class="w-10 h-10">...</svg>
```

---

This design system ensures consistency across your Nexus application while maintaining flexibility for different use cases. Always refer to this guide when implementing new features or components.

