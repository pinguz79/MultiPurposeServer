class Share {
    constructor(root) {
        this.root = root;
        this.button = root.querySelector('[data-share-action="toggle"]');
        this.menu = root.querySelector('[data-share-menu]');
        this.nativeButton = root.querySelector('[data-share-action="native"]');
        this.nativeHint = root.querySelector('[data-share-native-hint]');
        this.facebookLink = root.querySelector('[data-share-action="facebook"]');
        this.whatsappLink = root.querySelector('[data-share-action="whatsapp"]');
        this.telegramLink = root.querySelector('[data-share-action="telegram"]');
        this.emailLink = root.querySelector('[data-share-action="email"]');
        this.copyButton = root.querySelector('[data-share-action="copy"]');
        this.feedback = root.querySelector('[data-share-feedback]');
        this.feedbackTimer = null;

        if (!this.button || !this.menu) {
            return;
        }

        this.update(root.dataset.shareTitle ?? '', root.dataset.shareText ?? '', root.dataset.shareUrl ?? window.location.href);
        this.initialize();
        this.root.shareComponent = this;
    }

    initialize() {
        this.initializeNativeShare();
        this.initializeCopyLink();

        this.button.addEventListener('click', event => {
            event.stopPropagation();
            this.toggleMenu();
        });

        this.menu.addEventListener('click', event => event.stopPropagation());

        document.addEventListener('click', event => {
            if (!this.root.contains(event.target)) {
                this.closeMenu();
            }
        });

        document.addEventListener('keydown', event => {
            if (event.key === 'Escape' && !this.menu.hidden) {
                this.closeMenu();
                this.button.focus();
            }
        });
    }

    update(title, text, url) {
        this.title = title ?? '';
        this.text = text ?? '';
        this.url = new URL(url || window.location.href, window.location.origin).toString();

        this.root.dataset.shareTitle = this.title;
        this.root.dataset.shareText = this.text;
        this.root.dataset.shareUrl = this.url;

        this.updateLinks();
        this.closeMenu();
    }

    updateLinks() {
        const encodedUrl = encodeURIComponent(this.url);
        const encodedTitle = encodeURIComponent(this.title);
        const message = [this.title, this.text, this.url].filter(value => value.trim() !== '').join('\n');
        const encodedMessage = encodeURIComponent(message);

        if (this.facebookLink) {
            this.facebookLink.href = `https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`;
        }

        if (this.whatsappLink) {
            this.whatsappLink.href = `https://wa.me/?text=${encodedMessage}`;
        }

        if (this.telegramLink) {
            this.telegramLink.href = `https://t.me/share/url?url=${encodedUrl}&text=${encodedTitle}`;
        }

        if (this.emailLink) {
            this.emailLink.href = `mailto:?subject=${encodedTitle}&body=${encodedMessage}`;
        }
    }

    initializeNativeShare() {
        if (!this.nativeButton) {
            return;
        }

        if (typeof navigator.share !== 'function') {
            this.nativeButton.hidden = true;
            if (this.nativeHint) {
                this.nativeHint.hidden = true;
            }
            return;
        }

        this.nativeButton.hidden = false;
        if (this.nativeHint) {
            this.nativeHint.hidden = false;
        }

        this.nativeButton.addEventListener('click', async () => {
            try {
                await navigator.share({ title: this.title, text: this.text, url: this.url });
                this.closeMenu();
            }
            catch (error) {
                if (!(error instanceof DOMException && error.name === 'AbortError')) {
                    this.showFeedback('Impossibile condividere il contenuto.');
                }
            }
        });
    }

    initializeCopyLink() {
        if (!this.copyButton) {
            return;
        }

        this.copyButton.addEventListener('click', async () => {
            try {
                await this.copyToClipboard(this.url);
                this.showFeedback('Link copiato.');
                this.closeMenu();
            }
            catch {
                this.showFeedback('Impossibile copiare il link.');
            }
        });
    }

    async copyToClipboard(value) {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(value);
            return;
        }

        const textArea = document.createElement('textarea');
        textArea.value = value;
        textArea.setAttribute('readonly', '');
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';
        textArea.style.pointerEvents = 'none';

        document.body.appendChild(textArea);
        textArea.select();
        textArea.setSelectionRange(0, textArea.value.length);

        const copied = document.execCommand('copy');
        textArea.remove();

        if (!copied) {
            throw new Error('Copy command failed.');
        }
    }

    toggleMenu() {
        this.menu.hidden ? this.openMenu() : this.closeMenu();
    }

    openMenu() {
        this.menu.hidden = false;
        this.button.setAttribute('aria-expanded', 'true');
    }

    closeMenu() {
        this.menu.hidden = true;
        this.button.setAttribute('aria-expanded', 'false');
    }

    showFeedback(message) {
        if (!this.feedback) {
            return;
        }

        this.feedback.textContent = message;
        window.clearTimeout(this.feedbackTimer);

        this.feedbackTimer = window.setTimeout(() => {
            this.feedback.textContent = '';
        }, 2500);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.share').forEach(element => new Share(element));
});
