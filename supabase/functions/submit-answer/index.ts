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
    const body = await req.json();
    const token = typeof body.attemptToken === "string" ? body.attemptToken : "";
    const selectedIndex = body.selectedIndex;

    if (!token || !Number.isInteger(selectedIndex) || selectedIndex < 0 || selectedIndex > 5) {
      return jsonResponse({ error: "Invalid request" }, 400);
    }

    const supabase = createClient(supabaseUrl, serviceRoleKey, {
      auth: { persistSession: false, autoRefreshToken: false },
    });

    // Claim the token exactly once before reading the answer key.
    const tokenHash = await sha256(token);
    const { data: attempt, error: claimError } = await supabase
      .from("quiz_attempts")
      .update({ consumed_at: new Date().toISOString() })
      .eq("token_hash", tokenHash)
      .is("consumed_at", null)
      .gt("expires_at", new Date().toISOString())
      .select("question_id")
      .maybeSingle();

    if (claimError) throw claimError;
    if (!attempt) return jsonResponse({ error: "Attempt token is invalid, expired, or already used" }, 409);

    const { data: question, error: questionError } = await supabase
      .from("questions")
      .select("correct_index, explanation")
      .eq("id", attempt.question_id)
      .eq("verified", true)
      .maybeSingle();

    if (questionError) throw questionError;
    if (!question) return jsonResponse({ error: "Question no longer available" }, 404);

    return jsonResponse({
      isCorrect: selectedIndex === question.correct_index,
      explanation: question.explanation,
    });
  } catch (error) {
    console.error("submit-answer failed", error);
    return jsonResponse({ error: "Unable to submit answer" }, 500);
  }
});
