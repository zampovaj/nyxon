window.nyxonArgon2 = {
    deriveKey: async function (passB64, saltB64, length, iterations, memoryKb, parallelism) {
        // Decode Base64 → Uint8Array
        if (!window.argon2) throw new Error("argon2 not loaded");

        // argon2.hash returns a promise, so we await it
        const pass = Uint8Array.from(atob(passB64), c => c.charCodeAt(0));
        const salt = Uint8Array.from(atob(saltB64), c => c.charCodeAt(0));

        try {
            const result = await argon2.hash({
                pass,
                salt,
                type: argon2.ArgonType.Argon2id,
                hashLen: length,
                time: iterations,
                mem: memoryKb,
                parallelism
            });
            return btoa(String.fromCharCode(...result.hash));
        }
        finally {
            pass.fill(0);
            salt.fill(0);
        }
    }
};
