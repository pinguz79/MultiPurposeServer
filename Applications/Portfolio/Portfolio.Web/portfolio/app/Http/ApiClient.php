<?php

require_once __DIR__ . '/../Services/ApiCacheService.php';

class ApiClient
{
    public static function get(string $endpoint, ?int $ttlSeconds = null)
    {
        $url = API_BASE_URL . $endpoint;

        if ($ttlSeconds !== null && $ttlSeconds > 0) {
            $cache = new ApiCacheService();

            $cached = $cache->get($url);
            if ($cached !== null) {
                return $cached;
            }
        }

        $ch = curl_init();

        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_TIMEOUT, API_TIMEOUT);

        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);

        if (curl_errno($ch)) {
            curl_close($ch);
            return null;
        }

        curl_close($ch);

        if ($httpCode !== 200) {
            return null;
        }

        $decoded = json_decode($response, true);

        if (!is_array($decoded)) {
            return null;
        }

        if ($ttlSeconds !== null && $ttlSeconds > 0) {
            $cache->put($url, $decoded, $ttlSeconds, $httpCode);
        }

        return $decoded;
    }
}