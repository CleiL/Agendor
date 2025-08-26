PRAGMA foreign_keys = ON;

-- Usuários (autenticação)
CREATE TABLE IF NOT EXISTS Usuarios (
  UsuarioId    TEXT PRIMARY KEY,            -- GUID como string
  Email        TEXT NOT NULL UNIQUE,
  PasswordHash TEXT NOT NULL,
  Role         TEXT NOT NULL DEFAULT 'user',
  CreatedAt    TEXT NOT NULL DEFAULT (datetime('now'))
);

-- Pacientes
CREATE TABLE IF NOT EXISTS Pacientes (
  PacienteId   TEXT PRIMARY KEY,
  Nome         TEXT NOT NULL,
  CPF          TEXT NOT NULL,               -- valide no app
  Email        TEXT,
  Phone        TEXT,
  CreatedAt    TEXT NOT NULL DEFAULT (datetime('now')),
  UNIQUE (CPF)                               -- evita duplicado
);

-- Médicos
CREATE TABLE IF NOT EXISTS Medicos (
  MedicoId     TEXT PRIMARY KEY,
  Nome         TEXT NOT NULL,
  Email        TEXT,
  Phone        TEXT,
  CRM          TEXT NOT NULL,
  Especialidade TEXT NOT NULL,
  CreatedAt    TEXT NOT NULL DEFAULT (datetime('now')),
  UNIQUE (CRM)                               -- CRM único (ajuste se quiser CRM+UF)
);

-- Consultas
-- DataHora: início (ISO 'YYYY-MM-DD HH:MM')
-- DataHoraFim: gerada (início + 30 min)
CREATE TABLE IF NOT EXISTS Consultas (
  ConsultaId   TEXT PRIMARY KEY,
  DataHora     TEXT NOT NULL,  -- início
  DataFim  TEXT GENERATED ALWAYS AS (datetime(DataHora, '+30 minutes')) STORED,
  DataDia      TEXT GENERATED ALWAYS AS (date(DataHora)) STORED,
  PacienteId   TEXT NOT NULL,
  MedicoId     TEXT NOT NULL,

  -- FKs
  FOREIGN KEY (PacienteId) REFERENCES Pacientes(PacienteId) ON DELETE RESTRICT,
  FOREIGN KEY (MedicoId)   REFERENCES Medicos(MedicoId)     ON DELETE RESTRICT,

  -- Regras:
  -- 1) duração fixa de 30 minutos (assegurada pela coluna gerada)
  CHECK ( time(DataFim) = time(DataHora, '+30 minutes') ),

  -- 2) Janela de atendimento 08:00–18:00, logo início até 17:30
  CHECK ( time(DataHora) >= time('08:00') AND time(DataHora) <= time('17:30') ),

  -- 3) Apenas dias úteis (1..5, onde 0=Dom, 6=Sáb)
  CHECK ( CAST(strftime('%w', DataHora) AS INTEGER) BETWEEN 1 AND 5 )
);

-- “Um profissional só pode atender uma consulta por horário.”
CREATE UNIQUE INDEX IF NOT EXISTS UX_Consultas_Medico_Slot
  ON Consultas (MedicoId, DataHora);

-- “Um paciente só pode ter 1 consulta por profissional por dia.”
CREATE UNIQUE INDEX IF NOT EXISTS UX_Consultas_Paciente_Medico_Dia
  ON Consultas (PacienteId, MedicoId, date(DataHora));

-- Índices auxiliares
CREATE INDEX IF NOT EXISTS IX_Consultas_Data      ON Consultas (date(DataHora));
CREATE INDEX IF NOT EXISTS IX_Consultas_Paciente  ON Consultas (PacienteId);
CREATE INDEX IF NOT EXISTS IX_Consultas_Medico    ON Consultas (MedicoId);

-- View para listar agenda do médico
CREATE VIEW IF NOT EXISTS vw_AgendaMedico AS
SELECT c.ConsultaId, c.DataHora, c.DataHoraFim,
       m.MedicoId, m.Nome AS Medico, m.Especialidade,
       p.PacienteId, p.Nome AS Paciente
FROM Consultas c
JOIN Medicos  m ON m.MedicoId  = c.MedicoId
JOIN Pacientes p ON p.PacienteId = c.PacienteId;
