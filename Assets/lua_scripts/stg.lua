-- 共有する関数群。
local stg = {}

-- 指定されたフレーム数だけ待つ。
-- frames: 待機するフレーム数.
-- func: 待機中に実行する関数.
function stg:Wait(frames, func, ...)
	func = func or function() end  -- デフォルト引数。
	for i = 1, frames do
		func(...)  -- 引数が過剰な場合は無視されることに注意。
		coroutine.yield()
	end
end

local correspondingEffect = {
	--[EnemyID.SmallRed] = EffectID.RedCircle,
	--[EnemyID.SmallBlue] = EffectID.BlueCircle
}

-- 敵を生成し、それに消滅エフェクトを設定する。
-- id: 敵のID; initPosX: 初期位置のx座標; initPosY: 初期位置のy座標; speed: 初速度の大きさ; angle: 初速度の角度; hp: 体力.
function stg:CreateEnemy(id, initPosX, initPosY, speed, angle, hp)  -- 元々の ``GenerateEnemy'' と紛らわしい？
	local enemy = GenerateEnemy(id, initPosX, initPosY, speed, angle, hp)
	StartCoroutine(function()
		while enemy:IsEnabled() do
			coroutine.yield()
		end
		if enemy.HitPoint <= 0 then
			--GenerateEffect(correspondingEffect[id], enemy.PosX, enemy.PosY)
		end
	end)
	return enemy
end

-- 実行中の敵から弾を生成する。
-- id: 弾のID; enemy: 敵オブジェクト; speed: 初速度の大きさ; angle: 初速度の角度.
function stg:CreateBullet(id, enemy, speed, angle)
	local bullet = nil
	if enemy:IsEnabled() then
		bullet = GenerateBullet(id, enemy.Position.x, enemy.Position.y, speed, angle)
	end
	return bullet
end

-- 画面の左上と右上をa:bに内分する点の座標。
function stg:DivideInternallyScreenTop(a, b)
	local ScreenTopLeft = ScreenTop + (ScreenLeft - ScreenRight) * 0.5
	local ScreenTopRight = ScreenTop - (ScreenLeft - ScreenRight) * 0.5
	return ScreenTopLeft * b / (a + b) + ScreenTopRight * a / (a + b)
end

-- 或るオブジェクトから或るオブジェクトへの方向。第1引数を始点、第2引数を終点とする。
function stg:CalcAngleBetween(obj1, obj2)
	local relative = obj2.Position - obj1.Position
	return math.atan2(relative.y, relative.x)
end

return stg