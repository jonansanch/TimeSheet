globalThis.voiceInput = (() => {
    let recognition = null;
    let dotNetRef = null;
    let lang = 'es-CO';
    let stopped = false;
    let errorCount = 0;
    const MAX_ERRORS = 3;

    function isSupported() {
        return !!(globalThis.SpeechRecognition || globalThis.webkitSpeechRecognition);
    }

    function init() { /* no-op */ }

    function start(ref, language) {
        if (!isSupported()) return;
        dotNetRef = ref;
        lang = language || 'es-CO';
        errorCount = 0;
        stopped = false;

        if (recognition) {
            // Sesión anterior aún activa — esperamos su onend para arrancar la nueva
            const old = recognition;
            recognition = null;
            old.onerror = () => {};
            old.onend = () => { if (!stopped) startRecognition(); };
            old.stop();
            return;
        }
        startRecognition();
    }

    function startRecognition() {
        if (stopped) return;
        const SpeechRecognition = globalThis.SpeechRecognition || globalThis.webkitSpeechRecognition;
        const r = new SpeechRecognition();
        r.lang = lang;
        r.continuous = false;      // más estable en Chrome; auto-restart compensa
        r.interimResults = false;
        r.maxAlternatives = 1;

        r.onresult = (event) => {
            errorCount = 0;
            for (let i = event.resultIndex; i < event.results.length; i++) {
                if (event.results[i].isFinal) {
                    const transcript = event.results[i][0].transcript;
                    console.log('[voiceInput] segmento:', transcript);
                    dotNetRef.invokeMethodAsync('OnTranscriptReceived', transcript);
                }
            }
        };

        r.onerror = (event) => {
            console.error('[voiceInput] error:', event.error);
            recognition = null;
            if (stopped) return;

            if (event.error === 'no-speech') {
                // Silencio normal — reiniciar sin contar como error
                setTimeout(() => { if (!stopped) startRecognition(); }, 300);
                return;
            }

            errorCount++;
            if (errorCount < MAX_ERRORS) {
                console.log(`[voiceInput] reintentando tras ${event.error} (${errorCount}/${MAX_ERRORS})...`);
                setTimeout(() => { if (!stopped) startRecognition(); }, 1000);
            } else {
                dotNetRef.invokeMethodAsync('OnVoiceError', event.error);
            }
        };

        r.onend = () => {
            if (recognition !== r) return;
            recognition = null;
            if (stopped) {
                dotNetRef.invokeMethodAsync('OnRecognitionEnded');
            } else {
                // Auto-restart: el usuario sigue en modo escucha
                setTimeout(() => { if (!stopped) startRecognition(); }, 150);
            }
        };

        recognition = r;
        r.start();
    }

    function stop() {
        stopped = true;
        if (recognition) {
            recognition.stop();
            // No anular recognition aquí — onend lo hace y llama OnRecognitionEnded
        }
    }

    return { isSupported, init, start, stop };
})();
