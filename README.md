# Agendor

Desenvolver uma aplicação para gestão de agendamentos de consultas em uma clínica fictícia.

O sistema deve permitir:

Cadastro de pacientes
Cadastro de profissionais da saúde
Cadastro e agendamento de consultas
Visualização da agenda de um profissional
Login com autenticação via JWT

📘 Regras de Negócio

Um paciente só pode ter 1 consulta por profissional por dia.
Um profissional só pode atender uma consulta por horário.
O horário de atendimento é das 08:00 às 18:00, de segunda a sexta.
Consultas têm duração de 30 minutos.
O agendamento precisa validar disponibilidade antes de confirmar.
