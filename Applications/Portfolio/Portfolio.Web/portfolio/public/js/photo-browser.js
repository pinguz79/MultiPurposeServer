document.addEventListener('DOMContentLoaded', () => {
    const thumbnails = Array.from(document.querySelectorAll('.photo-thumbnail'));
    const previewFrame = document.querySelector('.photo-preview-frame');
    const previewImage = document.getElementById('photo-preview-image');
    const previewTitle = document.getElementById('photo-preview-title');
    const previewCounter = document.getElementById('photo-preview-counter');
    const previousButton = document.getElementById('previous-photo');
    const nextButton = document.getElementById('next-photo');
    const browserData = document.getElementById('photo-browser-data');

    const shareButton = document.getElementById('share-photo');
    const shareMenu = document.getElementById('photo-share-menu');
    const shareFacebook = document.getElementById('share-photo-facebook');
    const shareWhatsApp = document.getElementById('share-photo-whatsapp');
    const shareTelegram = document.getElementById('share-photo-telegram');
    const shareEmail = document.getElementById('share-photo-email');
    const copyLinkButton = document.getElementById('copy-photo-link');
    const shareFeedback = document.getElementById('photo-share-feedback');

    if (!thumbnails.length || !previewFrame || !previewImage || !previewTitle || !previewCounter || !previousButton || !nextButton || !browserData) {
        return;
    }

    const currentPage = Number.parseInt(browserData.dataset.currentPage ?? '1', 10);
    const totalPages = Number.parseInt(browserData.dataset.totalPages ?? '1', 10);
    const pageSize = Number.parseInt(browserData.dataset.pageSize ?? '12', 10);
    const totalPhotos = Number.parseInt(browserData.dataset.totalPhotos ?? '0', 10);
    const albumUrl = browserData.dataset.albumUrl ?? window.location.pathname;

    let selectedIndex = thumbnails.findIndex(thumbnail => thumbnail.classList.contains('selected'));
    let touchStartX = 0;
    let touchStartY = 0;
    let feedbackTimeout = null;

    const swipeThreshold = 50;
    const verticalTolerance = 80;

    if (selectedIndex < 0) {
        selectedIndex = 0;
    }

    const buildPageUrl = (page, photoId = null, select = null) => {
        const url = new URL(albumUrl, window.location.origin);

        url.searchParams.set('page', page);
        url.searchParams.set('pageSize', pageSize);

        if (photoId) {
            url.searchParams.set('photoId', photoId);
        }

        if (select) {
            url.searchParams.set('select', select);
        }

        return url.toString();
    };

    const getSelectedThumbnail = () => thumbnails[selectedIndex];

    const getSelectedPhotoData = () => {
        const thumbnail = getSelectedThumbnail();
        const photoId = thumbnail?.dataset.photoId ?? '';
        const photoName = thumbnail?.dataset.photoName ?? 'Fotografia';

        const url = new URL(window.location.href);
        url.searchParams.set('page', currentPage);
        url.searchParams.set('pageSize', pageSize);
        url.searchParams.set('photoId', photoId);
        url.searchParams.delete('select');

        return {
            id: photoId,
            name: photoName,
            url: url.toString()
        };
    };

    const updateBrowserUrl = photoId => {
        const url = new URL(window.location.href);

        url.searchParams.set('photoId', photoId);
        url.searchParams.delete('select');

        window.history.replaceState({}, '', url);
    };

    const updateNavigationButtons = () => {
        previousButton.disabled = selectedIndex === 0 && currentPage <= 1;
        nextButton.disabled = selectedIndex === thumbnails.length - 1 && currentPage >= totalPages;
    };

    const updatePhotoCounter = () => {
        const currentPhotoNumber = (currentPage - 1) * pageSize + selectedIndex + 1;
        previewCounter.textContent = `Foto ${currentPhotoNumber} di ${totalPhotos}`;
    };

    const preloadImage = imageUrl => {
        if (!imageUrl) {
            return;
        }

        const image = new Image();
        image.src = imageUrl;
    };

    const preloadAdjacentPhotos = () => {
        preloadImage(thumbnails[selectedIndex - 1]?.dataset.imageUrl ?? '');
        preloadImage(thumbnails[selectedIndex + 1]?.dataset.imageUrl ?? '');
    };

    const setShareFeedback = message => {
        if (!shareFeedback) {
            return;
        }

        window.clearTimeout(feedbackTimeout);
        shareFeedback.textContent = message;

        feedbackTimeout = window.setTimeout(() => {
            shareFeedback.textContent = '';
        }, 2500);
    };

    const closeShareMenu = () => {
        if (!shareMenu || !shareButton) {
            return;
        }

        shareMenu.hidden = true;
        shareButton.setAttribute('aria-expanded', 'false');
    };

    const openShareMenu = () => {
        if (!shareMenu || !shareButton) {
            return;
        }

        shareMenu.hidden = false;
        shareButton.setAttribute('aria-expanded', 'true');
    };

    const toggleShareMenu = () => {
        if (!shareMenu) {
            return;
        }

        if (shareMenu.hidden) {
            openShareMenu();
        }
        else {
            closeShareMenu();
        }
    };

    const updateShareLinks = () => {
        const photo = getSelectedPhotoData();
        const encodedUrl = encodeURIComponent(photo.url);
        const encodedText = encodeURIComponent(photo.name);
        const encodedMessage = encodeURIComponent(`${photo.name} - ${photo.url}`);

        if (shareFacebook) {
            shareFacebook.href = `https://www.facebook.com/sharer/sharer.php?u=${encodedUrl}`;
        }

        if (shareWhatsApp) {
            shareWhatsApp.href = `https://wa.me/?text=${encodedMessage}`;
        }

        if (shareTelegram) {
            shareTelegram.href = `https://t.me/share/url?url=${encodedUrl}&text=${encodedText}`;
        }

        if (shareEmail) {
            shareEmail.href = `mailto:?subject=${encodedText}&body=${encodedMessage}`;
        }
    };

    const copyText = async text => {
        if (navigator.clipboard && window.isSecureContext) {
            await navigator.clipboard.writeText(text);
            return;
        }

        const textArea = document.createElement('textarea');

        textArea.value = text;
        textArea.setAttribute('readonly', '');
        textArea.style.position = 'fixed';
        textArea.style.opacity = '0';

        document.body.appendChild(textArea);
        textArea.select();

        const copied = document.execCommand('copy');

        document.body.removeChild(textArea);

        if (!copied) {
            throw new Error('Impossibile copiare il link.');
        }
    };

    const shareSelectedPhoto = async () => {
        const photo = getSelectedPhotoData();
        const useNativeShare = typeof navigator.share === 'function' && window.matchMedia('(pointer: coarse)').matches;

        if (useNativeShare) {
            try {
                await navigator.share({
                    title: photo.name,
                    text: photo.name,
                    url: photo.url
                });
            }
            catch (error) {
                if (error.name !== 'AbortError') {
                    setShareFeedback('Condivisione non riuscita.');
                }
            }

            return;
        }

        updateShareLinks();
        toggleShareMenu();
    };

    const selectPhoto = (index, updateUrl = true) => {
        if (index < 0 || index >= thumbnails.length) {
            return;
        }

        selectedIndex = index;

        thumbnails.forEach((thumbnail, thumbnailIndex) => {
            const isSelected = thumbnailIndex === selectedIndex;

            thumbnail.classList.toggle('selected', isSelected);
            thumbnail.setAttribute('aria-current', isSelected ? 'true' : 'false');
        });

        const selectedThumbnail = getSelectedThumbnail();
        const photoId = selectedThumbnail.dataset.photoId ?? '';
        const photoName = selectedThumbnail.dataset.photoName ?? 'Fotografia';
        const photoAlt = selectedThumbnail.dataset.photoAlt ?? photoName;
        const imageUrl = selectedThumbnail.dataset.imageUrl ?? '';

        previewImage.src = imageUrl;
        previewImage.alt = photoAlt;
        previewTitle.textContent = photoName;

        updatePhotoCounter();
        updateNavigationButtons();
        preloadAdjacentPhotos();
        closeShareMenu();

        if (updateUrl && photoId) {
            updateBrowserUrl(photoId);
        }

        updateShareLinks();
    };

    const goToPreviousPhoto = () => {
        if (selectedIndex > 0) {
            selectPhoto(selectedIndex - 1);
            return;
        }

        if (currentPage > 1) {
            window.location.href = buildPageUrl(currentPage - 1, null, 'last');
        }
    };

    const goToNextPhoto = () => {
        if (selectedIndex < thumbnails.length - 1) {
            selectPhoto(selectedIndex + 1);
            return;
        }

        if (currentPage < totalPages) {
            window.location.href = buildPageUrl(currentPage + 1, null, 'first');
        }
    };

    const goToFirstPhoto = () => {
        if (currentPage === 1) {
            selectPhoto(0);
            return;
        }

        window.location.href = buildPageUrl(1, null, 'first');
    };

    const goToLastPhoto = () => {
        if (currentPage === totalPages) {
            selectPhoto(thumbnails.length - 1);
            return;
        }

        window.location.href = buildPageUrl(totalPages, null, 'last');
    };

    thumbnails.forEach((thumbnail, index) => {
        thumbnail.addEventListener('click', event => {
            event.preventDefault();
            selectPhoto(index);
        });
    });

    previousButton.addEventListener('click', goToPreviousPhoto);
    nextButton.addEventListener('click', goToNextPhoto);
    shareButton?.addEventListener('click', shareSelectedPhoto);

    copyLinkButton?.addEventListener('click', async () => {
        try {
            await copyText(getSelectedPhotoData().url);
            setShareFeedback('Link copiato.');
            closeShareMenu();
        }
        catch {
            setShareFeedback('Impossibile copiare il link.');
        }
    });

    document.addEventListener('click', event => {
        if (!shareMenu || shareMenu.hidden || !shareButton) {
            return;
        }

        const clickedInsideMenu = shareMenu.contains(event.target);
        const clickedShareButton = shareButton.contains(event.target);

        if (!clickedInsideMenu && !clickedShareButton) {
            closeShareMenu();
        }
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && shareMenu && !shareMenu.hidden) {
            closeShareMenu();
            shareButton?.focus();
            return;
        }

        const activeElement = document.activeElement;
        const isFormControl = activeElement instanceof HTMLInputElement
            || activeElement instanceof HTMLTextAreaElement
            || activeElement instanceof HTMLSelectElement
            || activeElement instanceof HTMLButtonElement;

        if (isFormControl) {
            return;
        }

        switch (event.key) {
            case 'ArrowLeft':
                event.preventDefault();
                goToPreviousPhoto();
                break;

            case 'ArrowRight':
                event.preventDefault();
                goToNextPhoto();
                break;

            case 'Home':
                event.preventDefault();
                goToFirstPhoto();
                break;

            case 'End':
                event.preventDefault();
                goToLastPhoto();
                break;
        }
    });

    previewFrame.addEventListener('touchstart', event => {
        const touch = event.changedTouches[0];

        touchStartX = touch.clientX;
        touchStartY = touch.clientY;
    }, { passive: true });

    previewFrame.addEventListener('touchend', event => {
        const touch = event.changedTouches[0];
        const deltaX = touch.clientX - touchStartX;
        const deltaY = touch.clientY - touchStartY;
        const isHorizontalSwipe = Math.abs(deltaX) >= swipeThreshold && Math.abs(deltaY) <= verticalTolerance;

        if (!isHorizontalSwipe) {
            return;
        }

        if (deltaX < 0) {
            goToNextPhoto();
        }
        else {
            goToPreviousPhoto();
        }
    }, { passive: true });

    const selectionMode = new URL(window.location.href).searchParams.get('select');

    if (selectionMode === 'last') {
        selectPhoto(thumbnails.length - 1);
    }
    else if (selectionMode === 'first') {
        selectPhoto(0);
    }
    else {
        selectPhoto(selectedIndex, false);
    }

    if (selectionMode) {
        const cleanUrl = new URL(window.location.href);

        cleanUrl.searchParams.delete('select');
        cleanUrl.searchParams.set('photoId', thumbnails[selectedIndex].dataset.photoId ?? '');

        window.history.replaceState({}, '', cleanUrl);
    }
});