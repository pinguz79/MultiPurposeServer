class PhotoBrowser {
    constructor(root) {
        this.root = root;
        this.thumbnails = Array.from(root.querySelectorAll('.photo-thumbnail'));
        this.previewFrame = root.querySelector('.photo-preview-frame');
        this.previewImage = root.querySelector('#photo-preview-image');
        this.previewTitle = root.querySelector('#photo-preview-title');
        this.previewCounter = root.querySelector('#photo-preview-counter');
        this.previousButton = root.querySelector('#previous-photo');
        this.nextButton = root.querySelector('#next-photo');
        this.browserData = root.querySelector('#photo-browser-data');
        this.photoShare = root.querySelector('.photo-preview-pane .share');

        this.selectedIndex = this.thumbnails.findIndex(thumbnail => thumbnail.classList.contains('selected'));
        this.touchStartX = 0;
        this.touchStartY = 0;

        this.swipeThreshold = 50;
        this.verticalTolerance = 80;

        if (!this.isValid()) {
            return;
        }

        if (this.selectedIndex < 0) {
            this.selectedIndex = 0;
        }

        this.currentPage = this.parseInteger(this.browserData.dataset.currentPage, 1);
        this.totalPages = this.parseInteger(this.browserData.dataset.totalPages, 1);
        this.pageSize = this.parseInteger(this.browserData.dataset.pageSize, 12);
        this.totalPhotos = this.parseInteger(this.browserData.dataset.totalPhotos, 0);
        this.albumUrl = this.browserData.dataset.albumUrl ?? window.location.pathname;
        this.photoShareText = this.photoShare?.dataset.shareText ?? 'Guarda questa fotografia di Marco Lepri Photography.';

        this.initialize();
    }

    isValid() {
        return this.thumbnails.length > 0
            && this.previewFrame
            && this.previewImage
            && this.previewTitle
            && this.previewCounter
            && this.previousButton
            && this.nextButton
            && this.browserData;
    }

    initialize() {
        this.initializeThumbnailNavigation();
        this.initializeButtons();
        this.initializeKeyboardNavigation();
        this.initializeTouchNavigation();
        this.initializeSelection();
    }

    initializeThumbnailNavigation() {
        this.thumbnails.forEach((thumbnail, index) => {
            thumbnail.addEventListener('click', event => {
                event.preventDefault();
                this.selectPhoto(index);
            });
        });
    }

    initializeButtons() {
        this.previousButton.addEventListener('click', () => this.goToPreviousPhoto());
        this.nextButton.addEventListener('click', () => this.goToNextPhoto());
    }

    initializeKeyboardNavigation() {
        document.addEventListener('keydown', event => {
            if (this.isFormControl(document.activeElement)) {
                return;
            }

            switch (event.key) {
                case 'ArrowLeft':
                    event.preventDefault();
                    this.goToPreviousPhoto();
                    break;

                case 'ArrowRight':
                    event.preventDefault();
                    this.goToNextPhoto();
                    break;

                case 'Home':
                    event.preventDefault();
                    this.goToFirstPhoto();
                    break;

                case 'End':
                    event.preventDefault();
                    this.goToLastPhoto();
                    break;
            }
        });
    }

    initializeTouchNavigation() {
        this.previewFrame.addEventListener('touchstart', event => {
            const touch = event.changedTouches[0];
            this.touchStartX = touch.clientX;
            this.touchStartY = touch.clientY;
        }, { passive: true });

        this.previewFrame.addEventListener('touchend', event => {
            const touch = event.changedTouches[0];
            const deltaX = touch.clientX - this.touchStartX;
            const deltaY = touch.clientY - this.touchStartY;
            const isHorizontalSwipe = Math.abs(deltaX) >= this.swipeThreshold
                && Math.abs(deltaY) <= this.verticalTolerance;

            if (!isHorizontalSwipe) {
                return;
            }

            deltaX < 0 ? this.goToNextPhoto() : this.goToPreviousPhoto();
        }, { passive: true });
    }

    initializeSelection() {
        const selectionMode = new URL(window.location.href).searchParams.get('select');

        if (selectionMode === 'last') {
            this.selectPhoto(this.thumbnails.length - 1);
        } else if (selectionMode === 'first') {
            this.selectPhoto(0);
        } else {
            this.selectPhoto(this.selectedIndex, false);
        }

        if (!selectionMode) {
            return;
        }

        const cleanUrl = new URL(window.location.href);
        cleanUrl.searchParams.delete('select');
        cleanUrl.searchParams.set('photoId', this.getSelectedThumbnail().dataset.photoId ?? '');

        window.history.replaceState({}, '', cleanUrl);
    }

    selectPhoto(index, updateUrl = true) {
        if (index < 0 || index >= this.thumbnails.length) {
            return;
        }

        this.selectedIndex = index;

        this.thumbnails.forEach((thumbnail, thumbnailIndex) => {
            const isSelected = thumbnailIndex === this.selectedIndex;

            thumbnail.classList.toggle('selected', isSelected);
            thumbnail.setAttribute('aria-current', isSelected ? 'true' : 'false');
        });

        const selectedThumbnail = this.getSelectedThumbnail();
        const photoId = selectedThumbnail.dataset.photoId ?? '';
        const photoName = selectedThumbnail.dataset.photoName ?? 'Fotografia';
        const photoAlt = selectedThumbnail.dataset.photoAlt ?? photoName;
        const imageUrl = selectedThumbnail.dataset.imageUrl ?? '';

        this.previewImage.src = imageUrl;
        this.previewImage.alt = photoAlt;
        this.previewTitle.textContent = photoName;

        this.updatePhotoCounter();
        this.updateNavigationButtons();
        this.updatePhotoShare(photoId, photoName);
        this.preloadAdjacentPhotos();

        if (updateUrl && photoId) {
            this.updateBrowserUrl(photoId);
        }
    }

    goToPreviousPhoto() {
        if (this.selectedIndex > 0) {
            this.selectPhoto(this.selectedIndex - 1);
            return;
        }

        if (this.currentPage > 1) {
            window.location.href = this.buildPageUrl(this.currentPage - 1, null, 'last');
        }
    }

    goToNextPhoto() {
        if (this.selectedIndex < this.thumbnails.length - 1) {
            this.selectPhoto(this.selectedIndex + 1);
            return;
        }

        if (this.currentPage < this.totalPages) {
            window.location.href = this.buildPageUrl(this.currentPage + 1, null, 'first');
        }
    }

    goToFirstPhoto() {
        if (this.currentPage === 1) {
            this.selectPhoto(0);
            return;
        }

        window.location.href = this.buildPageUrl(1, null, 'first');
    }

    goToLastPhoto() {
        if (this.currentPage === this.totalPages) {
            this.selectPhoto(this.thumbnails.length - 1);
            return;
        }

        window.location.href = this.buildPageUrl(this.totalPages, null, 'last');
    }

    updateNavigationButtons() {
        this.previousButton.disabled = this.selectedIndex === 0 && this.currentPage <= 1;
        this.nextButton.disabled = this.selectedIndex === this.thumbnails.length - 1 && this.currentPage >= this.totalPages;
    }

    updatePhotoCounter() {
        const currentPhotoNumber = (this.currentPage - 1) * this.pageSize + this.selectedIndex + 1;
        this.previewCounter.textContent = `Foto ${currentPhotoNumber} di ${this.totalPhotos}`;
    }

    updatePhotoShare(photoId, photoName) {
        if (!this.photoShare) {
            return;
        }

        const photoUrl = this.getSelectedPhotoUrl(photoId);

        if (this.photoShare.shareComponent) {
            this.photoShare.shareComponent.update(photoName, this.photoShareText, photoUrl);
            return;
        }

        this.photoShare.dataset.shareTitle = photoName;
        this.photoShare.dataset.shareText = this.photoShareText;
        this.photoShare.dataset.shareUrl = photoUrl;
    }

    updateBrowserUrl(photoId) {
        const url = new URL(window.location.href);

        url.searchParams.set('photoId', photoId);
        url.searchParams.delete('select');

        window.history.replaceState({}, '', url);
    }

    preloadAdjacentPhotos() {
        this.preloadImage(this.thumbnails[this.selectedIndex - 1]?.dataset.imageUrl ?? '');
        this.preloadImage(this.thumbnails[this.selectedIndex + 1]?.dataset.imageUrl ?? '');
    }

    preloadImage(imageUrl) {
        if (!imageUrl) {
            return;
        }

        const image = new Image();
        image.src = imageUrl;
    }

    getSelectedThumbnail() {
        return this.thumbnails[this.selectedIndex];
    }

    getSelectedPhotoUrl(photoId) {
        const url = new URL(this.albumUrl, window.location.origin);

        url.searchParams.set('page', this.currentPage);
        url.searchParams.set('pageSize', this.pageSize);
        url.searchParams.set('photoId', photoId);

        return url.toString();
    }

    buildPageUrl(page, photoId = null, select = null) {
        const url = new URL(this.albumUrl, window.location.origin);

        url.searchParams.set('page', page);
        url.searchParams.set('pageSize', this.pageSize);

        if (photoId) {
            url.searchParams.set('photoId', photoId);
        }

        if (select) {
            url.searchParams.set('select', select);
        }

        return url.toString();
    }

    parseInteger(value, fallback) {
        const parsedValue = Number.parseInt(value ?? '', 10);
        return Number.isNaN(parsedValue) ? fallback : parsedValue;
    }

    isFormControl(element) {
        return element instanceof HTMLInputElement
            || element instanceof HTMLTextAreaElement
            || element instanceof HTMLSelectElement
            || element instanceof HTMLButtonElement;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.photo-browser').forEach(element => new PhotoBrowser(element));
});
