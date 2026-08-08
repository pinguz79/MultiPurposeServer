<?php

declare(strict_types=1);

$advertisementContext ??= 'default';
?>

<aside class="advertisement advertisement--<?= htmlspecialchars($advertisementContext) ?>" aria-label="Pubblicità">
    <div class="advertisement-label">Pubblicità</div>

    <div class="advertisement-slot">
        <script>
            !function(d, l, e, s, c) {
                e = d.createElement('script');
                e.src = '//ad.altervista.org/js.ad/size=300X250/?ref=' + encodeURIComponent(l.hostname + l.pathname) + '&r=' + Date.now();
                s = d.scripts;
                c = d.currentScript || s[s.length - 1];
                c.parentNode.insertBefore(e, c);
            }(document, location);
        </script>
    </div>
</aside>
