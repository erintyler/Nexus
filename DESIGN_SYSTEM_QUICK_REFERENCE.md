# Nexus Design System - Quick Reference ✨

> **Modern. Sleek. Personality.** A vibrant design system for the next generation.

## 🎨 Most Common Class Combinations

### Form Input (Standard) - Glass Morphism Style
```html
class="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-gray-200 rounded-xl shadow-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all duration-200 hover:shadow-md"
```

### Form Input (Error State)
```html
class="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border-2 border-red-400 rounded-xl shadow-sm focus:ring-2 focus:ring-red-500 outline-none transition-all duration-200"
```

### Primary Button - Gradient with Glow
```html
class="px-6 py-3 bg-gradient-to-r from-blue-600 to-indigo-600 text-white font-semibold rounded-xl hover:from-blue-700 hover:to-indigo-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-lg hover:shadow-xl"
```

### Success Button - Vibrant Gradient
```html
class="px-6 py-3 bg-gradient-to-r from-emerald-500 to-teal-600 text-white font-semibold rounded-xl hover:from-emerald-600 hover:to-teal-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-lg hover:shadow-xl"
```

### Secondary Button - Ghost Style
```html
class="px-6 py-3 bg-gray-50 text-gray-700 font-semibold rounded-xl hover:bg-gray-100 border-2 border-gray-200 hover:border-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200"
```

### Danger/Destructive Button
```html
class="px-6 py-3 bg-gradient-to-r from-red-500 to-pink-600 text-white font-semibold rounded-xl hover:from-red-600 hover:to-pink-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-lg hover:shadow-xl"
```

### Small Button - Accent
```html
class="px-4 py-2 bg-gradient-to-r from-violet-500 to-purple-600 text-white font-medium rounded-lg hover:from-violet-600 hover:to-purple-700 focus:outline-none focus:ring-2 focus:ring-violet-500 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-md"
```

### Disabled Button
```html
disabled class="px-6 py-3 bg-gray-300 text-gray-500 font-semibold rounded-xl cursor-not-allowed opacity-60"
```

### Card - Modern Elevated
```html
class="bg-white/90 backdrop-blur-sm shadow-xl rounded-2xl p-6 border border-gray-100 hover:shadow-2xl transition-shadow duration-300"
```

### Card - Gradient Border
```html
class="bg-white rounded-2xl p-6 shadow-lg hover:shadow-xl transition-all duration-300 border-2 border-transparent bg-clip-padding relative before:absolute before:inset-0 before:rounded-2xl before:p-[2px] before:bg-gradient-to-r before:from-blue-500 before:to-purple-600 before:-z-10"
```

### Tag/Badge - Vibrant Pills
```html
class="inline-flex items-center bg-gradient-to-r from-blue-500 to-indigo-500 text-white text-sm font-medium px-4 py-1.5 rounded-full shadow-md"
```

### Tag/Badge - Soft Glow
```html
class="inline-flex items-center bg-blue-100 text-blue-700 text-sm font-medium px-4 py-1.5 rounded-full border border-blue-200 shadow-sm"
```

### Success Alert - Modern
```html
class="p-4 rounded-xl bg-gradient-to-r from-emerald-50 to-teal-50 text-emerald-800 border-l-4 border-emerald-500 shadow-md backdrop-blur-sm"
```

### Error Alert - Modern
```html
class="p-4 rounded-xl bg-gradient-to-r from-red-50 to-pink-50 text-red-800 border-l-4 border-red-500 shadow-md backdrop-blur-sm"
```

### Warning Alert - Modern
```html
class="p-4 rounded-xl bg-gradient-to-r from-amber-50 to-yellow-50 text-amber-800 border-l-4 border-amber-500 shadow-md backdrop-blur-sm"
```

### Info Alert - Modern
```html
class="p-4 rounded-xl bg-gradient-to-r from-blue-50 to-indigo-50 text-blue-800 border-l-4 border-blue-500 shadow-md backdrop-blur-sm"
```

---

## 📝 Common Patterns

### Form Field - Modern Glass Morphism
```html
<div class="mb-6">
    <label for="id" class="block text-sm font-semibold text-gray-700 mb-2">
        Label <span class="text-red-500">*</span>
    </label>
    <input 
        type="text" 
        id="id"
        class="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-gray-200 rounded-xl shadow-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all duration-200 hover:shadow-md"
        placeholder="Placeholder..." />
    <p class="text-xs text-gray-500 mt-2">Helper text</p>
</div>
```

### Button Group - Vibrant
```html
<div class="flex gap-3">
    <button class="flex-1 px-6 py-3 bg-gradient-to-r from-emerald-500 to-teal-600 text-white font-semibold rounded-xl hover:from-emerald-600 hover:to-teal-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200 shadow-lg hover:shadow-xl">
        Primary Action
    </button>
    <button class="px-6 py-3 bg-gray-50 text-gray-700 font-semibold rounded-xl hover:bg-gray-100 border-2 border-gray-200 hover:border-gray-300 focus:outline-none focus:ring-2 focus:ring-gray-400 focus:ring-offset-2 transform hover:scale-105 active:scale-95 transition-all duration-200">
        Cancel
    </button>
</div>
```

### Loading Spinner - Colorful Gradient
```html
<div class="relative">
    <svg class="animate-spin h-8 w-8" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" class="text-gray-300"></circle>
        <path class="opacity-75" fill="url(#gradient)" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        <defs>
            <linearGradient id="gradient" x1="0%" y1="0%" x2="100%" y2="100%">
                <stop offset="0%" style="stop-color:#3b82f6;stop-opacity:1" />
                <stop offset="100%" style="stop-color:#8b5cf6;stop-opacity:1" />
            </linearGradient>
        </defs>
    </svg>
</div>
```

### Animated Card - Hover Effect
```html
<div class="group relative bg-white rounded-2xl p-6 shadow-lg hover:shadow-2xl transition-all duration-300 overflow-hidden">
    <!-- Gradient overlay on hover -->
    <div class="absolute inset-0 bg-gradient-to-br from-blue-500/10 to-purple-500/10 opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
    
    <div class="relative z-10">
        <h3 class="text-xl font-bold text-gray-800 mb-2 group-hover:text-blue-600 transition-colors duration-300">Card Title</h3>
        <p class="text-gray-600">Card content with personality!</p>
    </div>
</div>
```

---

## 🎯 Modern Color Palette

### Gradient Buttons
| Purpose | Gradient | Hover |
|---------|----------|-------|
| **Primary** | `from-blue-600 to-indigo-600` | `from-blue-700 to-indigo-700` |
| **Success** | `from-emerald-500 to-teal-600` | `from-emerald-600 to-teal-700` |
| **Danger** | `from-red-500 to-pink-600` | `from-red-600 to-pink-700` |
| **Accent** | `from-violet-500 to-purple-600` | `from-violet-600 to-purple-700` |
| **Warning** | `from-amber-500 to-orange-600` | `from-amber-600 to-orange-700` |

### Alert/Message Colors (Modern Gradients)
| Type | Background Gradient | Text | Border |
|------|---------------------|------|--------|
| **Success** | `from-emerald-50 to-teal-50` | `text-emerald-800` | `border-emerald-500` |
| **Error** | `from-red-50 to-pink-50` | `text-red-800` | `border-red-500` |
| **Warning** | `from-amber-50 to-yellow-50` | `text-amber-800` | `border-amber-500` |
| **Info** | `from-blue-50 to-indigo-50` | `text-blue-800` | `border-blue-500` |

### Tag/Badge Colors
| Type | Vibrant (Gradient) | Soft (Solid) |
|------|-------------------|--------------|
| **Primary** | `from-blue-500 to-indigo-500` | `bg-blue-100 text-blue-700` |
| **Success** | `from-emerald-500 to-teal-500` | `bg-emerald-100 text-emerald-700` |
| **Warning** | `from-amber-500 to-orange-500` | `bg-amber-100 text-amber-700` |
| **Danger** | `from-red-500 to-pink-500` | `bg-red-100 text-red-700` |
| **Accent** | `from-violet-500 to-purple-500` | `bg-violet-100 text-violet-700` |

---

## 📏 Spacing Scale

| Class | Value | Pixels | Usage |
|-------|-------|--------|-------|
| `p-2` / `m-2` | 0.5rem | 8px | Tight spacing |
| `p-3` / `m-3` | 0.75rem | 12px | Default gaps |
| `p-4` / `m-4` | 1rem | 16px | Standard padding |
| `p-6` / `m-6` | 1.5rem | 24px | Form fields, cards |
| `gap-2` | 0.5rem | 8px | Tight gaps |
| `gap-3` | 0.75rem | 12px | Button groups |
| `gap-4` | 1rem | 16px | Card grids |
| `gap-6` | 1.5rem | 24px | Large gaps |

---

## 🔤 Typography

| Element | Classes | Example |
|---------|---------|---------|
| **Page Title** | `text-2xl font-bold text-gray-800` | Main heading |
| **Section Title** | `text-xl font-bold text-gray-800` | Section heading |
| **Subsection** | `text-lg font-semibold text-gray-800` | Subsection |
| **Body Text** | `text-base text-gray-700` | Paragraph |
| **Small Text** | `text-sm text-gray-600` | Labels, secondary |
| **Helper Text** | `text-xs text-gray-500` | Hints, captions |
| **Error Text** | `text-sm text-red-600` | Error messages |

---

## 🎲 Icon Sizes

```html
<!-- Small (inline) -->
<svg class="w-4 h-4">...</svg>

<!-- Medium (default) -->
<svg class="w-5 h-5">...</svg>

<!-- Large (decorative) -->
<svg class="w-10 h-10">...</svg>
```

---

## 📦 Layout Containers

```html
<!-- Standard (centered, max-width) -->
<div class="max-w-2xl mx-auto p-6">
    <!-- Content -->
</div>

<!-- Wide -->
<div class="max-w-6xl mx-auto p-6">
    <!-- Content -->
</div>

<!-- Full Width -->
<div class="w-full p-6">
    <!-- Content -->
</div>
```

---

## 🔗 Focus States

Always include focus states on interactive elements:

```html
focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2
```

For inputs:
```html
focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none
```

---

## ✨ Transitions

Common transitions:

```html
<!-- Colors -->
transition-colors

<!-- Shadow -->
transition-shadow

<!-- All properties -->
transition-all
```

---

## 📱 Responsive Utilities

```html
<!-- Mobile first approach -->
class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3"

<!-- Hide on mobile -->
class="hidden md:block"

<!-- Stack on mobile, row on desktop -->
class="flex flex-col md:flex-row"
```

---

## 🎯 Copy-Paste Components

### Complete Form Field with Modern Styling
```html
<div class="mb-6">
    <label for="field-id" class="block text-sm font-semibold text-gray-700 mb-2 flex items-center gap-2">
        Field Label <span class="text-red-500">*</span>
    </label>
    <input 
        type="text" 
        id="field-id"
        class="w-full px-4 py-3 bg-white/80 backdrop-blur-sm border border-gray-200 rounded-xl shadow-sm focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none transition-all duration-200 hover:shadow-md"
        placeholder="Enter value..." />
    <p class="text-red-600 text-sm mt-2 font-medium">Error message here</p>
</div>
```

### Modern Alert with Icon and Gradient
```html
<div class="p-4 rounded-xl bg-gradient-to-r from-emerald-50 to-teal-50 text-emerald-800 border-l-4 border-emerald-500 shadow-md backdrop-blur-sm">
    <div class="flex items-start gap-3">
        <div class="flex-shrink-0">
            <svg class="w-6 h-6 text-emerald-600" fill="currentColor" viewBox="0 0 20 20">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clip-rule="evenodd"></path>
            </svg>
        </div>
        <div class="flex-1">
            <h4 class="font-semibold text-emerald-900 mb-1">Success!</h4>
            <p class="text-sm">Your action was completed successfully.</p>
        </div>
    </div>
</div>
```

### Vibrant Tag with Remove Button
```html
<div class="inline-flex items-center bg-gradient-to-r from-blue-500 to-indigo-500 text-white text-sm font-medium px-4 py-1.5 rounded-full shadow-md hover:shadow-lg transition-shadow duration-200">
    <span class="font-semibold">Type:</span>
    <span class="ml-1">Value</span>
    <button type="button" class="ml-2 text-white/90 hover:text-white focus:outline-none transition-colors">
        <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
            <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
        </svg>
    </button>
</div>
```

### Interactive Card with Hover Effects
```html
<div class="group relative bg-white rounded-2xl p-6 shadow-lg hover:shadow-2xl transition-all duration-300 overflow-hidden cursor-pointer">
    <!-- Animated gradient overlay -->
    <div class="absolute inset-0 bg-gradient-to-br from-blue-500/10 to-purple-500/10 opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
    
    <!-- Content -->
    <div class="relative z-10">
        <div class="flex items-center justify-between mb-3">
            <h3 class="text-lg font-bold text-gray-800 group-hover:text-blue-600 transition-colors duration-300">Card Title</h3>
            <span class="text-gray-400 group-hover:text-blue-500 transition-colors">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path>
                </svg>
            </span>
        </div>
        <p class="text-gray-600 text-sm">Card content with modern styling and smooth animations!</p>
    </div>
</div>
```

### Modern File Upload Zone
```html
<label class="group relative flex flex-col items-center justify-center w-full h-64 border-2 border-dashed border-gray-300 rounded-2xl cursor-pointer bg-gradient-to-br from-gray-50 to-white hover:from-blue-50 hover:to-indigo-50 hover:border-blue-400 transition-all duration-300">
    <div class="flex flex-col items-center justify-center pt-5 pb-6">
        <div class="mb-4 p-4 bg-gradient-to-br from-blue-500 to-indigo-500 rounded-full group-hover:scale-110 transition-transform duration-300">
            <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12"></path>
            </svg>
        </div>
        <p class="mb-2 text-sm text-gray-600 font-semibold">
            <span class="text-blue-600">Click to upload</span> or drag and drop
        </p>
        <p class="text-xs text-gray-500">PNG, JPG, GIF up to 10MB</p>
    </div>
    <input type="file" class="hidden" accept="image/*" />
</label>
```

---

## 🚀 Quick Tips

1. **Always use focus states** - Accessibility is important
2. **Add transitions** - Makes the UI feel smoother
3. **Consistent spacing** - Use mb-6 for form fields, gap-3 for buttons
4. **Semantic colors** - Green = success, Red = error, Blue = primary
5. **Mobile first** - Design for mobile, enhance for desktop
6. **Test keyboard navigation** - All interactive elements should be keyboard accessible

---

## 📚 Resources

- **Full Documentation**: See `DESIGN_SYSTEM.md`
- **Live Examples**: Navigate to `/design-system` in the app
- **Icon Library**: Use Heroicons (https://heroicons.com/)
- **Tailwind Docs**: https://tailwindcss.com/docs

---

**Last Updated**: December 30, 2025

