window.audioRecorder = {
    mediaRecorder: null,
    audioChunks: [],

    start: async function () {
        this.audioChunks = [];
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);

            this.mediaRecorder.ondataavailable = event => {
                this.audioChunks.push(event.data);
            };

            this.mediaRecorder.start();
        } catch (err) {
            console.error("Microphone access denied or unavailable", err);
            alert("Please allow microphone access to send voice notes.");
        }
    },

    stopAndGetBase64: function () {
        return new Promise((resolve) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === "inactive") {
                resolve("");
                return;
            }

            this.mediaRecorder.onstop = () => {
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/webm' });

                
                const reader = new FileReader();
                reader.readAsDataURL(audioBlob);
                reader.onloadend = () => {
                    resolve(reader.result); 
                };

                // Stop all mic tracks to turn off the red recording light in browser tab
                this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
            };

            this.mediaRecorder.stop();
        });
    }
};