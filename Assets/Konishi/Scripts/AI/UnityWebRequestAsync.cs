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

        // すでに終わっている場合もあるので、completed を使って一度だけ結果を渡す
        operation.completed += _ => completion.TrySetResult(request);

        if (cancellationToken.CanBeCanceled)
        {
            cancellationToken.Register(() =>
            {
                if (completion.Task.IsCompleted) return;

                request.Abort();
                completion.TrySetResult(request);
            });
        }

        return completion.Task;
    }
}
