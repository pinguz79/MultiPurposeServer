<?php

declare(strict_types=1);

require_once __DIR__ . '/../Views/Models/PageMetadataFactory.php';

class AboutController
{
    public function index(): void
    {
        $pageMetadata = PageMetadataFactory::about();
        $view = __DIR__ . '/../Views/About/index.php';

        require __DIR__ . '/../Views/Layout/main.php';
    }
}
