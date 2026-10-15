using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

/// <summary>
/// UnityWebRequest を await できるようにする小さな補助。
///
/// Tomo注: Unityの通信はコルーチン前提ですが、指示書どおり Task で扱いたいので包んでいます。
///         ここは仕組みのためだけのコードで、ゲームの仕様は入っていません。
/// </summary>
public static class UnityWebRequestAsync
{
    public static Task<UnityWebRequest> SendAsync(this UnityWebRequest request, CancellationToken cancellationToken = default)
    {
        var completion = new TaskCompletionSource<UnityWebRequest>();
        var operation = request.SendWebRequest();

        // キャンセル登録は使い終わったら必ず解放する。放置すると
        // CancellationTokenSource が生きている間ずっと溜まり続ける。
        CancellationTokenRegistration registration = default;

        operation.completed += _ =>
        {
            registration.Dispose();
            completion.TrySetResult(request);
        };

        if (cancellationToken.CanBeCanceled)
        {
            registration = cancellationToken.Register(() =>
            {
                if (completion.Task.IsCompleted) return;

                // 通信を打ち切る。応答は捨てる。
                request.Abort();
                completion.TrySetResult(request);
            });
        }

        return completion.Task;
    }
}
