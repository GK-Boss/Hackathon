-- One-time answer tokens. The client never receives correct_index.
-- Edge Functions use the service-role key; client roles receive no table access.

create table if not exists public.quiz_attempts (
  id uuid primary key default gen_random_uuid(),
  question_id text not null references public.questions(id) on delete cascade,
  token_hash text not null unique,
  created_at timestamptz not null default now(),
  expires_at timestamptz not null default (now() + interval '20 minutes'),
  consumed_at timestamptz
);

create index if not exists quiz_attempts_expires_at_idx
  on public.quiz_attempts (expires_at);

alter table public.quiz_attempts enable row level security;

revoke all on table public.quiz_attempts from anon, authenticated;

-- Remove expired records periodically (for example, with Supabase Cron).
-- Do not create a client-facing policy for this table.
