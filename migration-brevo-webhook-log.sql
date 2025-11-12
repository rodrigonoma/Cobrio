START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251111213412_AdicionarTabelaBrevoWebhookLog') THEN

    CREATE TABLE `BrevoWebhookLog` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `EventoTipo` longtext CHARACTER SET utf8mb4 NOT NULL,
        `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
        `MessageId` longtext CHARACTER SET utf8mb4 NULL,
        `BrevoEventId` bigint NOT NULL,
        `PayloadCompleto` longtext CHARACTER SET utf8mb4 NOT NULL,
        `EnderecoIp` longtext CHARACTER SET utf8mb4 NULL,
        `UserAgent` longtext CHARACTER SET utf8mb4 NULL,
        `Headers` longtext CHARACTER SET utf8mb4 NULL,
        `ProcessadoComSucesso` tinyint(1) NOT NULL,
        `MensagemErro` longtext CHARACTER SET utf8mb4 NULL,
        `HistoricoNotificacaoId` char(36) COLLATE ascii_general_ci NULL,
        `DataEvento` datetime(6) NOT NULL,
        `DataRecebimento` datetime(6) NOT NULL,
        `CriadoEm` datetime(6) NOT NULL,
        `AtualizadoEm` datetime(6) NOT NULL,
        `UsuarioCriacaoId` char(36) COLLATE ascii_general_ci NULL,
        `UsuarioModificacaoId` char(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_BrevoWebhookLog` PRIMARY KEY (`Id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251111213412_AdicionarTabelaBrevoWebhookLog') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251111213412_AdicionarTabelaBrevoWebhookLog', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

