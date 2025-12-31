// JavaScript module for NexusDropdown component - handles click outside functionality

let dropdownElement = null;
let dotNetHelper = null;
let clickOutsideHandler = null;

export function initializeDropdown(element, dotNetRef) {
    dropdownElement = element;
    dotNetHelper = dotNetRef;
}

export function addClickOutsideListener(element, dotNetRef) {
    // Update references in case they changed
    dropdownElement = element;
    dotNetHelper = dotNetRef;
    
    // Remove any existing listener first
    removeClickOutsideListener();
    
    // Small delay to avoid immediate triggering from the click that opened the dropdown
    setTimeout(() => {
        clickOutsideHandler = (event) => {
            if (dropdownElement && !dropdownElement.contains(event.target)) {
                if (dotNetHelper) {
                    dotNetHelper.invokeMethodAsync('CloseDropdownFromJs');
                }
            }
        };
        
        document.addEventListener('click', clickOutsideHandler);
    }, 10);
}

export function removeClickOutsideListener() {
    if (clickOutsideHandler) {
        document.removeEventListener('click', clickOutsideHandler);
        clickOutsideHandler = null;
    }
}
