/**
 * Teclado Virtual Inteligente para .NET MVC
 * Ubicación sugerida: wwwroot/js/virtual-keyboard.js
 */
(function () {
    let activeInput = null;
    let isShift = false;
    let isSymbols = false;
    let isDragging = false;
    let currentX, currentY, initialX, initialY, xOffset = 0, yOffset = 0;
    let container = null;

    // Layouts de teclado
    const layouts = {
        alpha: [
            ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P'],
            ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Ñ'],
            ['SHIFT', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', 'BACK'],
            ['?123', 'SPACE', 'ENTER']
        ],
        symbols: [
            ['@', '#', '$', '%', '&', '-', '+', '(', ')', '/'],
            ['*', '"', "'", ':', ';', '!', '?', '_', ',', '.'],
            ['=', '<', '>', '[', ']', '{', '}', '\\', 'BACK'],
            ['ABC', 'SPACE', 'ENTER']
        ],
        numpad: {
            // Layout de 4 columnas (Grid)
            numeric: ['7', '8', '9', '+', '4', '5', '6', '1', '2', '3', '*', '0', '.', 'ENTER'],
            integer: ['7', '8', '9', '+', '4', '5', '6', '1', '2', '3', '*', '0', 'ENTER'], // 0 será h2 mediante lógica en render
            tel:     ['7', '8', '9', '+', '4', '5', '6', '1', '2', '3', '-', '0', '*', 'ENTER']
        }
    };

    function init() {
        // Crear el contenedor si no existe
        if (!document.getElementById('virtual-keyboard')) {
            container = document.createElement('div');
            container.id = 'virtual-keyboard';
            container.className = 'vk-container';
            document.body.appendChild(container);
            
            // Eventos globales para el arrastre
            document.addEventListener("mousedown", dragStart);
            document.addEventListener("mousemove", drag);
            document.addEventListener("mouseup", dragEnd);
            
            // Eventos táctiles para móviles
            document.addEventListener("touchstart", dragStart, { passive: false });
            document.addEventListener("touchmove", drag, { passive: false });
            document.addEventListener("touchend", dragEnd);
        }

        // Escuchar focus y click en todo el documento para máxima compatibilidad con modales dinámicos
        document.addEventListener('focusin', handleGlobalFocus);
        document.addEventListener('mousedown', function (e) {
            if (e.target && e.target.classList && e.target.classList.contains('jsteclado')) {
                handleGlobalFocus(e);
            }
        });
    }

    function handleGlobalFocus(e) {
        const target = e.target;
        if (target && target.classList && target.classList.contains('jsteclado')) {
            if (activeInput === target && container.style.display === 'flex') return;
            
            activeInput = target;
            
            const isDecimal = target.classList.contains('jsdecimal') || target.getAttribute('inputmode') === 'decimal';
            const isInteger = target.classList.contains('jsinteger');
            const isNum = target.type === 'number' || 
                          target.type === 'tel' || 
                          target.getAttribute('inputmode') === 'numeric' ||
                          isDecimal || isInteger;
            
            let type = 'alphanumeric';
            if (isInteger) {
                type = 'integer';
            } else if (target.type === 'tel') {
                type = 'tel';
            } else if (isNum) {
                type = 'numeric';
            }

            isShift = false;
            isSymbols = false;
            container.dataset.type = type;
            render(type);
            container.style.display = 'flex';
            container.style.opacity = '1';
        }
    }

    function render(type) {
        let isAlphanumeric = (type === 'alphanumeric');
        
        // Manejar clase para modo solo numérico
        if (isAlphanumeric) {
            container.classList.remove('vk-numpad-only');
        } else {
            container.classList.add('vk-numpad-only');
        }

        let title = 'Teclado Alfanumérico';
        if (type === 'numeric') title = 'Teclado Decimal';
        if (type === 'integer') title = 'Teclado Entero';
        if (type === 'tel') title = 'Teclado Telefónico';

        let html = `
            <div class="vk-header" id="vk-header">
                <div class="vk-drag-handle"></div>
                <div class="vk-title-bar">
                    <span class="vk-title">${title}</span>
                    <span class="vk-close" id="vk-close-btn">✕</span>
                </div>
            </div>
            <div class="vk-body">
        `;

        // Render Alpha section if needed
        if (isAlphanumeric) {
            let alphaLayout = isSymbols ? layouts.symbols : layouts.alpha;
            html += '<div class="vk-alpha-section">';
            alphaLayout.forEach(row => {
                html += '<div class="vk-row">';
                row.forEach(key => {
                    let className = 'vk-key';
                    let label = key;

                    if (key === 'SHIFT') {
                        className += ' vk-key-special' + (isShift ? ' active-shift' : '');
                        className += ' vk-key-w1-5';
                        label = '⇧';
                    } else if (key === 'BACK') {
                        className += ' vk-key-backspace vk-key-w1-5';
                        label = '⌫';
                    } else if (key === 'ENTER') {
                        className += ' vk-key-enter vk-key-w2';
                        label = 'ENTER';
                    } else if (key === 'SPACE') {
                        className += ' vk-key-space';
                        label = 'Espacio';
                    } else if (key === '?123' || key === 'ABC') {
                        className += ' vk-key-special vk-key-w2';
                        label = key;
                    } else {
                        label = isShift ? key.toUpperCase() : key.toLowerCase();
                    }
                    html += `<div class="${className}" data-key="${key}">${label}</div>`;
                });
                html += '</div>';
            });
            html += '</div>';
            html += '<div class="vk-separator"></div>';
        }

        // Render Numpad section
        const numpadKey = isAlphanumeric ? 'numeric' : type;
        const numLayout = layouts.numpad[numpadKey];
        html += '<div class="vk-numpad-section">';
        numLayout.forEach(key => {
            let className = 'vk-key';
            let label = key;

            if (key === '+') className += ' vk-key-v2';
            if (key === '0' && type === 'integer') className += ' vk-key-h2';
            if (key === 'ENTER') {
                className += ' vk-key-h2 vk-key-enter';
                label = 'ENTER';
            }
            if (key === 'BACK') className += ' vk-key-backspace';

            html += `<div class="${className}" data-key="${key}">${label}</div>`;
        });
        html += '</div>';

        html += '</div>';
        container.innerHTML = html;

        // Asignar eventos a las teclas
        container.querySelectorAll('.vk-key').forEach(btn => {
            btn.addEventListener('mousedown', (e) => {
                e.preventDefault(); // Evita que el input pierda el foco
                handleKeyPress(btn.dataset.key);
            });
            btn.addEventListener('touchstart', (e) => {
                e.preventDefault();
                handleKeyPress(btn.dataset.key);
            });
        });

        document.getElementById('vk-close-btn').onclick = () => {
            container.style.display = 'none';
        };
    }

    function handleKeyPress(key) {
        if (!activeInput || key === 'SEP') return;

        const start = activeInput.selectionStart ?? activeInput.value.length;
        const end = activeInput.selectionEnd ?? activeInput.value.length;
        const val = activeInput.value;
        const isDecimal = activeInput.classList.contains('jsdecimal') || activeInput.type === 'number' || activeInput.getAttribute('inputmode') === 'decimal';
        const isInteger = activeInput.classList.contains('jsinteger');

        if (key === 'SHIFT') {
            isShift = !isShift;
            const currentType = container.dataset.type;
            render(currentType);
            return;
        }

        if (key === '?123') {
            isSymbols = true;
            render('alphanumeric');
            return;
        }

        if (key === 'ABC') {
            isSymbols = false;
            render('alphanumeric');
            return;
        }

        let newValue = val;
        let newCursorPos = start;

        if (key === 'BACK') {
            if (start === end) {
                newValue = val.substring(0, start - 1) + val.substring(end);
                newCursorPos = Math.max(0, start - 1);
            } else {
                newValue = val.substring(0, start) + val.substring(end);
                newCursorPos = start;
            }
        } else if (key === 'ENTER') {
            if (activeInput.tagName === 'TEXTAREA') {
                newValue = val.substring(0, start) + '\n' + val.substring(end);
                activeInput.value = newValue;
                const newPos = start + 1;
                activeInput.setSelectionRange(newPos, newPos);
                activeInput.dispatchEvent(new Event('input', { bubbles: true }));
                return;
            }

            const inputs = Array.from(document.querySelectorAll('.jsteclado'));
            const currentIndex = inputs.indexOf(activeInput);
            
            // Despachar eventos de teclado para compatibilidad (keydown, keypress, keyup)
            const eventProps = {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true,
                cancelable: true
            };
            
            const keydownEvt = new KeyboardEvent('keydown', eventProps);
            const keypressEvt = new KeyboardEvent('keypress', eventProps);
            activeInput.dispatchEvent(keydownEvt);
            activeInput.dispatchEvent(keypressEvt);
            
            if (currentIndex > -1 && currentIndex < inputs.length - 1) {
                // Navegar al siguiente campo compatible
                const nextInput = inputs[currentIndex + 1];
                nextInput.focus();
            } else {
                // Cerrar teclado si es el último o no hay más
                container.style.display = 'none';
                activeInput.blur();
            }
            
            const keyupEvt = new KeyboardEvent('keyup', eventProps);
            activeInput.dispatchEvent(keyupEvt);
            return;
        } else if (key === 'SPACE') {
            if (isInteger || isDecimal) return; // No espacios en números
            newValue = val.substring(0, start) + ' ' + val.substring(end);
            newCursorPos = start + 1;
        } else if (key === '.') {
            if (isInteger) return; // No puntos en enteros
            if (isDecimal && val.includes('.')) return;
            newValue = val.substring(0, start) + '.' + val.substring(end);
            newCursorPos = start + 1;
        } else {
            const char = isShift ? key.toUpperCase() : key.toLowerCase();
            newValue = val.substring(0, start) + char + val.substring(end);
            newCursorPos = start + 1;
        }

        activeInput.value = newValue;
        
        // Intentar mantener el foco y la posición del cursor
        try {
            activeInput.setSelectionRange(newCursorPos, newCursorPos);
        } catch(e) {}
        
        activeInput.focus();
        activeInput.dispatchEvent(new Event('input', { bubbles: true }));
    }

    // Lógica de Arrastre (Drag & Drop)
    function dragStart(e) {
        const header = e.target.closest('#vk-header');
        if (header) {
            const clientX = e.type === "touchstart" ? e.touches[0].clientX : e.clientX;
            const clientY = e.type === "touchstart" ? e.touches[0].clientY : e.clientY;
            initialX = clientX - xOffset;
            initialY = clientY - yOffset;
            isDragging = true;
        }
    }

    function drag(e) {
        if (isDragging) {
            if (e.cancelable) e.preventDefault();
            const clientX = e.type === "touchmove" ? e.touches[0].clientX : e.clientX;
            const clientY = e.type === "touchmove" ? e.touches[0].clientY : e.clientY;
            currentX = clientX - initialX;
            currentY = clientY - initialY;
            xOffset = currentX;
            yOffset = currentY;
            container.style.transform = `translate(calc(-50% + ${currentX}px), ${currentY}px)`;
        }
    }

    function dragEnd() {
        isDragging = false;
    }

    // Iniciar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
