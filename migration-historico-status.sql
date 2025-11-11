START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251111141003_AdicionarTabelaHistoricoStatusNotificacao') THEN

    CREATE TABLE `HistoricoStatusNotificacao` (
        `Id` char(36) COLLATE ascii_general_ci NOT NULL,
        `HistoricoNotificacaoId` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `StatusAnterior` int NOT NULL,
        `StatusNovo` int NOT NULL,
        `DataMudanca` datetime(6) NOT NULL,
        `Detalhes` longtext CHARACTER SET utf8mb4 NULL,
        `IpOrigem` longtext CHARACTER SET utf8mb4 NULL,
        `UserAgent` longtext CHARACTER SET utf8mb4 NULL,
        `CriadoEm` datetime(6) NOT NULL,
        `AtualizadoEm` datetime(6) NOT NULL,
        CONSTRAINT `PK_HistoricoStatusNotificacao` PRIMARY KEY (`Id`),
        CONSTRAINT `FK_HistoricoStatusNotificacao_HistoricoNotificacao_HistoricoNot~` FOREIGN KEY (`HistoricoNotificacaoId`) REFERENCES `HistoricoNotificacao` (`Id`) ON DELETE CASCADE
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
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251111141003_AdicionarTabelaHistoricoStatusNotificacao') THEN

    CREATE INDEX `IX_HistoricoStatusNotificacao_HistoricoNotificacaoId` ON `HistoricoStatusNotificacao` (`HistoricoNotificacaoId`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20251111141003_AdicionarTabelaHistoricoStatusNotificacao') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20251111141003_AdicionarTabelaHistoricoStatusNotificacao', '8.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

