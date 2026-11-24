-- Выполнить один раз в базе SoundLabs через pgAdmin / Query Tool.
-- Добавляет статусы доставки и сохраняет статусы самовывоза.

BEGIN;

DO $$
DECLARE c RECORD;
BEGIN
    -- Удаляем старое CHECK-ограничение, которое ограничивало список статусов заказа.
    FOR c IN
        SELECT conname
        FROM pg_constraint
        WHERE conrelid = 'public.orders'::regclass
          AND contype = 'c'
          AND pg_get_constraintdef(oid) ILIKE '%status%'
    LOOP
        EXECUTE format('ALTER TABLE public.orders DROP CONSTRAINT %I', c.conname);
    END LOOP;
END $$;

ALTER TABLE public.orders
    ADD CONSTRAINT chk_orders_status
    CHECK (status IN (
        'Новый',
        'Подтверждён',
        'В пути',
        'Доставлен',
        'Готов к выдаче',
        'Выполнен',
        'Отклонён'
    ));

COMMIT;
