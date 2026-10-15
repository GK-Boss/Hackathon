using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// 問題の保存・取得の窓口。
///
/// Tomo注: QuestionManager は「問題がどこに保存されているか」を知りません。
///         いまはローカルのJSON（LocalJsonQuestionRepository）ですが、
///         将来この実装をサーバー通信版に差し替えれば、QuestionManager は無修正で動きます。
///
///         企画書どおり、最終構成は Unity → HTTPS → 自前サーバー → 問題DB です。
///         JSONは「Unityとサーバーの間のデータ形式」であって、JSONファイル自体がDBではありません。
/// </summary>
public interface IQuestionRepository
{
    /// 読み込みを済ませる。サーバー版では通信が走る。
    Task InitializeAsync();

    Task<QuestionData> GetQuestionAsync(string id);

    /// 指定した関連語を持つ問題をすべて取得する。
    Task<List<QuestionData>> GetQuestionsByRelatedTermAsync(string relatedTerm, string subject = null);

    /// 指定した関連語に紐づく問題数。1/Nルールの N にあたる。
    Task<int> CountQuestionsByRelatedTermAsync(string relatedTerm, string subject = null);

    /// 分野の問題をすべて取得する。subject が null なら全分野。
    Task<List<QuestionData>> GetQuestionsBySubjectAsync(string subject, bool verifiedOnly = false);

    /// <summary>
    /// 問題を保存する。すでに同じ問題があれば false。
    /// 企画書どおり、今回のプレイで使わなかった問題も保存する。
    /// </summary>
    Task<bool> SaveQuestionAsync(QuestionData question);

    /// 登録されている分野名の一覧。
    Task<List<string>> GetSubjectsAsync();
}
