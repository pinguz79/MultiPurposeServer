<?php

declare(strict_types=1);

// Secrets.php viene caricato esplicitamente perché ApiClient usa le credenziali di autenticazione.
// La configurazione generale dell'applicazione viene invece caricata durante il bootstrap.
require_once __DIR__ . '/../Config/Secrets.php';
require_once __DIR__ . '/../Services/ApiCacheService.php';

class ApiClient {
    public static function get(string $endpoint, ?int $ttlSeconds = null): ?array {
        $url = API_BASE_URL . $endpoint;
        $cache = null;

        if ($ttlSeconds !== null && $ttlSeconds > 0) {
            $cache = new ApiCacheService();
            $cached = $cache->get($url);

            if ($cached !== null) {
                return $cached;
            }
        }

        $ch = curl_init();

        curl_setopt_array($ch, [
            CURLOPT_URL => $url,
            CURLOPT_RETURNTRANSFER => true,
            CURLOPT_TIMEOUT => API_TIMEOUT,
            CURLOPT_HTTPHEADER => [
                'Accept: application/json',
                'X-Portfolio-Api-Key: ' . PORTFOLIO_FRONTEND_API_KEY
            ]
        ]);

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);

        if (curl_errno($ch)) {
            AppLogger::write(sprintf('[Portfolio ApiClient] Errore cURL durante la chiamata a %s: %s', $url, curl_error($ch)));
            curl_close($ch);
            return null;
        }

        curl_close($ch);

        if ($httpCode < 200 || $httpCode >= 300) {
            AppLogger::write(sprintf('[Portfolio ApiClient] GET %s ha restituito HTTP %d.', $url, $httpCode));
            return null;
        }

        try {
            $decoded = json_decode($response, true, 512, JSON_THROW_ON_ERROR);

            if ($cache !== null) {
                $cache->put($url, $decoded, $ttlSeconds, $httpCode);
            }

            return $decoded;
        } catch (JsonException) {
            AppLogger::write(sprintf('[Portfolio ApiClient] GET %s ha restituito JSON non valido.', $url));
            return null;
        }
    }
}
