import { createClient } from "npm:@supabase/supabase-js@2";
import { handleOptions, jsonResponse, sha256 } from "../_shared/http.ts";

const supabaseUrl = Deno.env.get("SUPABASE_URL")!;
const serviceRoleKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!;

Deno.serve(async (req) => {
  const early = handleOptions(req);
  if (early) return early;

  if (!supabaseUrl || !serviceRoleKey) {
    return jsonResponse({ error: "Server configuration error" }, 500);
  }

  try {
    const body = await req.json().catch(() => ({}));
    const subject = typeof body.subject === "string" && body.subject.trim()
      ? body.subject.trim()
      : null;

    const supabase = createClient(supabaseUrl, serviceRoleKey, {
      auth: { persistSession: false, autoRefreshToken: false },
    });

    // correct_index and explanation are deliberately not selected here.
    let query = supabase
      .from("questions")
      .select("id, subject, difficulty, question_text, choices, related_terms")
      .eq("verified", true);

    if (subject) query = query.eq("subject", subject);

    const { data, error } = await query.limit(500);
    if (error) throw error;
    if (!data?.length) return jsonResponse({ error: "No verified question found" }, 404);

    const question = data[Math.floor(Math.random() * data.length)];
    const attemptToken = crypto.randomUUID();
    const tokenHash = await sha256(attemptToken);

    const { error: insertError } = await supabase.from("quiz_attempts").insert({
      question_id: question.id,
      token_hash: tokenHash,
    });
    if (insertError) throw insertError;

    return jsonResponse({
      question: {
        id: question.id,
        subject: question.subject,
        difficulty: question.difficulty,
        questionText: question.question_text,
        choices: question.choices,
        relatedTerms: question.related_terms,
      },
      attemptToken,
    });
  } catch (error) {
    console.error("get-question failed", error);
    return jsonResponse({ error: "Unable to get question" }, 500);
  }
});
