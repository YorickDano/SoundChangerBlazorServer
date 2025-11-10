// Initialize Audio Context
const audioContext = new (window.AudioContext || window.webkitAudioContext)();
const audioElement = document.getElementById("audioPlayer");
const track = audioContext.createMediaElementSource(audioElement);

// Create gain node for volume control
const gainNode = audioContext.createGain();

// Create panner node for stereo panning
const panner = new StereoPannerNode(audioContext, { pan: 0 });

// Connect nodes: source -> gain -> panner -> destination
track.connect(gainNode).connect(panner).connect(audioContext.destination);

// Play button functionality
const playButton = document.getElementById("buttonPlayer");
playButton.addEventListener("click", () => {
    if (audioContext.state === "suspended") {
        audioContext.resume();
    }

    if (playButton.dataset.playing === "false") {
        audioElement.play();
        playButton.dataset.playing = "true";
        playButton.textContent = "Pause";
        setupVisualizer(); // Start visualizer when playing
    } else if (playButton.dataset.playing === "true") {
        audioElement.pause();
        playButton.dataset.playing = "false";
        playButton.textContent = "Play";
    }
}, false);

// Volume control
const volumeControl = document.getElementById("volume");
const volumeValue = document.getElementById("volume-value");

volumeControl.addEventListener("input", () => {
    gainNode.gain.value = volumeControl.value;
    volumeValue.textContent = volumeControl.value;
}, false);

// Panner control
const pannerControl = document.getElementById("panner");
const pannerValue = document.getElementById("panner-value");

pannerControl.addEventListener("input", () => {
    panner.pan.value = pannerControl.value;
    pannerValue.textContent = pannerControl.value;
}, false);

// Tempo control - This is what you asked about
const tempoControl = document.getElementById("tempo");
const tempoValue = document.getElementById("tempo-value");

tempoControl.addEventListener("input", () => {
    // Adjust playback rate for tempo control
    audioElement.playbackRate = parseFloat(tempoControl.value);
    tempoValue.textContent = tempoControl.value + "x";
}, false);

// Setup audio visualizer
const visualizer = document.getElementById('visualizer');
const canvasCtx = visualizer.getContext('2d');
let analyzer;

function setupVisualizer() {
    if (analyzer) {
        analyzer.disconnect();
    }

    analyzer = audioContext.createAnalyser();
    analyzer.fftSize = 256;
    panner.connect(analyzer);

    const bufferLength = analyzer.frequencyBinCount + 50;
    const dataArray = new Uint8Array(bufferLength);

    function draw() {
        if (audioElement.paused) return;

        requestAnimationFrame(draw);

        analyzer.getByteFrequencyData(dataArray);

        canvasCtx.fillStyle = 'rgba(0, 0, 0, 0.2)';
        canvasCtx.fillRect(0, 0, visualizer.width, visualizer.height);

        const barWidth = (visualizer.width / bufferLength) * 2.5;
        let barHeight;
        let x = 0;

        for (let i = 0; i < bufferLength; i++) {
            barHeight = dataArray[i] / 2;

            canvasCtx.fillStyle = `rgb(50, ${barHeight + 100}, ${(barHeight + 100) / 2})`;
            canvasCtx.fillRect(x, visualizer.height - barHeight, barWidth, barHeight);

            x += barWidth + 1;
        }
    }

    draw();
}

// Handle audio context suspension
audioElement.addEventListener('ended', () => {
    playButton.dataset.playing = "false";
    playButton.textContent = "Play";
});
