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
        alphanumeric: [
            ['Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'SEP', '7', '8', '9', 'BACK'],
            ['A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Ñ', 'SEP', '4', '5', '6', '+'],
            ['SHIFT', 'Z', 'X', 'C', 'V', 'B', 'N', 'M', 'BACK', 'SEP', '1', '2', '3', '*'],
            ['?123', 'SPACE', 'SEP', '0', '.', 'ENTER']
        ],
        symbols: [
            ['@', '#', '$', '%', '&', '-', '+', '(', ')', '/', 'SEP', '7', '8', '9', 'BACK'],
            ['*', '"', "'", ':', ';', '!', '?', '_', ',', '.', 'SEP', '4', '5', '6', '+'],
            ['=', '<', '>', '[', ']', '{', '}', '\\', 'BACK', 'SEP', '1', '2', '3', '*'],
            ['ABC', 'SPACE', 'SEP', '0', '.', 'ENTER']
        ],
        numeric: [
            ['7', '8', '9', 'BACK'],
            ['4', '5', '6', '+'],
            ['1', '2', '3', '*'],
            ['0', '.', 'ENTER']
        ],
        integer: [
            ['7', '8', '9', 'BACK'],
            ['4', '5', '6', '+'],
            ['1', '2', '3', '*'],
            ['0', 'ENTER']
        ],
        tel: [
            ['7', '8', '9', 'BACK'],
            ['4', '5', '6', '+'],
            ['1', '2', '3', '-'],
            ['0', '*', 'ENTER']
        ]
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

        // Escuchar focus en todo el documento
        document.addEventListener('focusin', handleGlobalFocus);
    }

    function handleGlobalFocus(e) {
        const target = e.target;
        if (target.classList.contains('jsteclado')) {
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
        let layout = layouts[type];
        if (type === 'alphanumeric' && isSymbols) {
            layout = layouts.symbols;
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

        layout.forEach((row, rowIndex) => {
            html += '<div class="vk-row">';
            row.forEach(key => {
                let className = 'vk-key';
                let label = key;

                if (key === 'SEP') {
                    className = 'vk-separator';
                    label = '';
                } else if (key === 'SHIFT') {
                    className += ' vk-key-special' + (isShift ? ' active-shift' : '');
                    if (type === 'alphanumeric') className += ' vk-key-w1-5';
                    label = '⇧';
                } else if (key === 'BACK') {
                    className += ' vk-key-backspace';
                    if (type === 'alphanumeric' || type === 'symbols') className += ' vk-key-w1-5';
                    label = '⌫';
                } else if (key === 'ENTER') {
                    className += ' vk-key-enter';
                    if (type === 'alphanumeric' || type === 'symbols' || type === 'numeric' || type === 'tel') {
                        className += ' vk-key-w2';
                    } else if (type === 'integer') {
                        className += ' vk-key-w2';
                    }

                    // Dinámicamente decidir el label
                    const inputs = Array.from(document.querySelectorAll('.jsteclado'));
                    const currentIndex = inputs.indexOf(activeInput);
                    const hasNext = currentIndex > -1 && currentIndex < inputs.length - 1;
                    label = hasNext ? 'Sig.' : 'Listo';
                } else if (key === 'SPACE') {
                    className += ' vk-key-space';
                    label = 'Espacio';
                } else if (key === '?123' || key === 'ABC') {
                    className += ' vk-key-special vk-key-w2';
                    label = key;
                } else if (key === '0' && (type === 'alphanumeric' || type === 'symbols')) {
                    className += ' vk-key-w1'; 
                } else if (key === '0' && (type === 'numeric' || type === 'tel') && rowIndex === 3) {
                    className += ' vk-key-w1';
                } else if (key === '0' && type === 'integer' && rowIndex === 3) {
                    className += ' vk-key-w2'; 
                } else {
                    label = isShift ? key.toUpperCase() : key.toLowerCase();
                }

                html += `<div class="${className}" data-key="${key}">${label}</div>`;
            });
            html += '</div>';
        });

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
            const inputs = Array.from(document.querySelectorAll('.jsteclado'));
            const currentIndex = inputs.indexOf(activeInput);
            
            // Despachar eventos de teclado para compatibilidad
            const enterEvt = {
                key: 'Enter',
                code: 'Enter',
                keyCode: 13,
                which: 13,
                bubbles: true
            };
            activeInput.dispatchEvent(new KeyboardEvent('keydown', enterEvt));
            
            if (currentIndex > -1 && currentIndex < inputs.length - 1) {
                // Navegar al siguiente campo
                const nextInput = inputs[currentIndex + 1];
                nextInput.focus();
            } else {
                // Cerrar teclado si es el último
                container.style.display = 'none';
                activeInput.blur();
            }
            
            activeInput.dispatchEvent(new KeyboardEvent('keyup', enterEvt));
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
