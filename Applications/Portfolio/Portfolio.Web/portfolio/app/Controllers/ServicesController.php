<?php

declare(strict_types=1);

require_once __DIR__ . '/../Views/Models/PageMetadataFactory.php';

class ServicesController {
    public function index(): void {
        $pageMetadata = PageMetadataFactory::services();
        $view = __DIR__ . '/../Views/Services/index.php';

        require __DIR__ . '/../Views/Layout/main.php';
    }
}
