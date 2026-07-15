document.addEventListener('DOMContentLoaded', () => {
    const thumbnails = Array.from(document.querySelectorAll('.photo-thumbnail'));
    const previewImage = document.getElementById('photo-preview-image');
    const previewTitle = document.getElementById('photo-preview-title');
    const previousButton = document.getElementById('previous-photo');
    const nextButton = document.getElementById('next-photo');
    const browserData = document.getElementById('photo-browser-data');

    if (!thumbnails.length || !previewImage || !previewTitle || !previousButton || !nextButton || !browserData) {
        return;
    }

    const currentPage = Number.parseInt(browserData.dataset.currentPage ?? '1', 10);
    const totalPages = Number.parseInt(browserData.dataset.totalPages ?? '1', 10);
    const pageSize = Number.parseInt(browserData.dataset.pageSize ?? '12', 10);
    const albumUrl = browserData.dataset.albumUrl ?? window.location.pathname;

    let selectedIndex = thumbnails.findIndex(thumbnail => thumbnail.classList.contains('selected'));

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

        const selectedThumbnail = thumbnails[selectedIndex];
        const photoId = selectedThumbnail.dataset.photoId ?? '';
        const photoName = selectedThumbnail.dataset.photoName ?? 'Fotografia';
        const photoAlt = selectedThumbnail.dataset.photoAlt ?? photoName;
        const imageUrl = selectedThumbnail.dataset.imageUrl ?? '';

        previewImage.src = imageUrl;
        previewImage.alt = photoAlt;
        previewTitle.textContent = photoName;

        if (updateUrl && photoId) {
            updateBrowserUrl(photoId);
        }

        updateNavigationButtons();
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

    thumbnails.forEach((thumbnail, index) => {
        thumbnail.addEventListener('click', event => {
            event.preventDefault();
            selectPhoto(index);
        });
    });

    previousButton.addEventListener('click', goToPreviousPhoto);
    nextButton.addEventListener('click', goToNextPhoto);

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