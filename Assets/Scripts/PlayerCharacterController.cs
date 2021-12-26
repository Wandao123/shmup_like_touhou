using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerCharacterController : Player
{
    [SerializeField]
    private float highSpeed = 0.0f;
    [SerializeField]
    private float lowSpeed = 0.0f;
    [SerializeField]
    private string path = "";
    private Sprite[,] clips;
    private Rigidbody2D rigid2D;

    public override Vector2 Velocity
    {
        get => base.Velocity;
        set => base.Velocity = value.normalized * (SlowMode ? lowSpeed : highSpeed);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // アニメーション。
        spriteRenderer.sprite = clipFromImage(Time.frameCount);

        // 描画の前処理。
        invincibleCounter = (invincibleCounter > 0) ? invincibleCounter - 1 : 0;
        if (spriteRenderer.color.a < 1.0f) {
            var color = spriteRenderer.color;
            if (invincibleCounter == 0)
                color.a = 1.0f;
            else if (invincibleCounter / 3 % 2 == 0)  // 3フレーム毎に点滅する。Sinを使ったらどうか？
                color.a = 0.0f;
            else
                color.a = 0.75f;
        }
    }

    void FixedUpdate()
    {
        // パラメータの更新。
        rigid2D.velocity = Velocity;  // 1フレームを単位時間とする。跳ね返るような挙動をするので、Translateを使った方が良い？
        disableIfOutside();

        // 移動制限。ただし、速度を変化させた場合のみで、位置を直接変える場合は制限しない。
        // HACK: 定数はMoverクラスと共通化したい。
        float width = spriteRenderer.bounds.size.x;
        float height = spriteRenderer.bounds.size.y;
        Vector2 minimum = mainCamera.ViewportToWorldPoint(new Vector2(0, 0)) + (new Vector3(width, height, 0) * 0.5f);
        Vector2 maximum = mainCamera.ViewportToWorldPoint(new Vector2(1, 1)) - (new Vector3(width, height, 0) * 0.5f);
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, minimum.x, maximum.x);
        position.y = Mathf.Clamp(position.y, minimum.y, maximum.y);
        transform.position = position;
    }

    // 参考：https://tsubakit1.hateblo.jp/entry/2019/06/18/000323
    //       https://light11.hatenadiary.com/entry/2019/12/30/222302
    //       https://light11.hatenadiary.com/entry/2019/03/06/002800
    async void Awake()
    {
        var spritesList = await Addressables.LoadAssetAsync<IList<Sprite>>(path).Task;
        clips = new Sprite[3, 5];
        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 5; j++)
                clips[i, j] = spritesList[5 * i + j];
        //spriteRenderer = GetComponent<SpriteRenderer>();
        //mainCamera = Camera.main;
        rigid2D = GetComponent<Rigidbody2D>();
        clipFromImage = clipFromImageClosure();
        this.enabled = true;
    }

    private Func<int, Sprite> clipFromImageClosure()
    {
        const int DelayFrames = 3;  // 左右移動の際に次の画像に切り替わるまでのフレーム数。
        const int NumSlice = 5;     // 停止時、左移動、右移動における各変化のコマ数。
        const int Period = 6;       // 停止時などに画像を繰り返す周期。
        int level = 0;              // 左右に何フレーム進んでいるか表すフラグ。-(DelayFrames * NumSlice - 1) .. DelayFrames * NumSlice - 1 の範囲を動く。
        return (int countedFrames) =>
        {
            if (Velocity.x < 0.0)
                level = Mathf.Max(level - 1, -(DelayFrames * NumSlice - 1));
            else if (Velocity.x > 0.0)
                level = Mathf.Min(level + 1, DelayFrames * NumSlice - 1);
            else
                level = (level != 0) ? (level - level / Mathf.Abs(level)) : 0;

            if (level == 0)
                return clips[0, (countedFrames / Period) % NumSlice];
            else if (level == -(DelayFrames * NumSlice - 1))  // 4コマ目と5コマ目だけ繰り返し。
                return clips[1, (countedFrames / Period) % 2 + 3];
            else if (level == DelayFrames * NumSlice - 1)
                return clips[2, (countedFrames / Period) % 2 + 3];
            else if (level < 0)
                return clips[1, -level / DelayFrames];
            else
                return clips[2, level / DelayFrames];
        };
    }
}
