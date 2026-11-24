-- Выполнить один раз в базе данных SoundLabs перед запуском обновлённого API.
ALTER TABLE products ADD COLUMN IF NOT EXISTS image_url VARCHAR(500);
