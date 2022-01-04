-- 共有する関数群。
local stg = {}

-- 指定されたフレーム数だけ待つ。
-- frames: 待機するフレーム数.
function stg:Wait(frames)
	for i = 1, frames do
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

return stg