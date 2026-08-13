<?php

declare(strict_types=1);

require_once __DIR__ . '/../Database/Db.php';

class ApiCacheService {
    public function get(string $url): ?array {
        $db = Db::connection();

        $stmt = $db->prepare("
            SELECT response_json, http_code
            FROM pw_api_response_cache
            WHERE cache_key = :cache_key
              AND expires_at > NOW()
            LIMIT 1
        ");

        $stmt->execute([':cache_key' => $this->key($url)]);

        $row = $stmt->fetch();

        if (!$row) {
            return null;
        }

        $httpCode = (int)$row['http_code'];

        if ($httpCode < 200 || $httpCode >= 300) {
            return null;
        }

        $decoded = json_decode($row['response_json'], true);
        return is_array($decoded) ? $decoded : null;
    }

    public function put(string $url, array $response, int $ttlSeconds, int $httpCode = 200): void {
        $db = Db::connection();
        $responseJson = json_encode($response, JSON_UNESCAPED_UNICODE | JSON_THROW_ON_ERROR);

        $stmt = $db->prepare("
            INSERT INTO pw_api_response_cache
                (cache_key, request_url, response_json, http_code, created_at, updated_at, expires_at)
            VALUES
                (:cache_key, :request_url, :response_json, :http_code, NOW(), NOW(), DATE_ADD(NOW(), INTERVAL :ttl SECOND))
            ON DUPLICATE KEY UPDATE
                request_url = VALUES(request_url),
                response_json = VALUES(response_json),
                http_code = VALUES(http_code),
                updated_at = NOW(),
                expires_at = VALUES(expires_at)
        ");

        $stmt->execute([
            ':cache_key' => $this->key($url),
            ':request_url' => $url,
            ':response_json' => $responseJson,
            ':http_code' => $httpCode,
            ':ttl' => $ttlSeconds
        ]);
    }

    public function delete(string $url): void {
        $db = Db::connection();

        $stmt = $db->prepare("
            DELETE FROM pw_api_response_cache
            WHERE cache_key = :cache_key
        ");

        $stmt->execute([':cache_key' => $this->key($url)]);
    }

    public function clear(): int {
        return Db::connection()->exec("DELETE FROM pw_api_response_cache");
    }

    private function key(string $url): string {
        return hash('sha256', $url);
    }
}
