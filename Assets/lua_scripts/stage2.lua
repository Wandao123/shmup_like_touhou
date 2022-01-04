local stage = {}

local stg = require('scripts.stg')

-- 真っ直ぐ降りてきて、自機狙いの分裂弾を放つ敵。
-- enemyColor: 敵の色; initPosX: 初期位置.
local function Fission(enemyColor, initPosX)
	local enemy = stg:CreateEnemy(enemyColor, initPosX, 0, 1.5, math.pi / 2, 80)
	stg:Wait(90)
	enemy.Speed = 0
	stg:Wait(5)
	local bulletColor = (enemyColor == EnemyID.SmallBlue) and BulletID.MiddleBlue or BulletID.MiddleRed
	for i = 1, 8 do
		local bullet = GenerateBullet(bulletColor, enemy, 6, math.pi / 2)
		stg:Wait(10)
		if bullet ~= nil then
			bullet:Erase()
			local angle = math.pi / 2 - math.atan(GetPlayer().PosX - bullet.PosX, GetPlayer().PosY - bullet.PosY)  -- 自機の方向。
			GenerateBullet(bulletColor, bullet.PosX, bullet.PosY, 6, angle + math.pi / 6)
			GenerateBullet(bulletColor, bullet.PosX, bullet.PosY, 6, angle)
			GenerateBullet(bulletColor, bullet.PosX, bullet.PosY, 6, angle - math.pi / 6)
			GenerateEffect(EffectID.EnemyShotSound)
		end
		stg:Wait(15)
	end
	local dir = (initPosX < ScreenWidth / 2) and 1 or -1  -- 初期位置が左寄りなら右向きに、右寄りなら左向きに進む。
	enemy.Speed = 3
	for i = 0, 5 do
		enemy.Angle = math.pi / 2 - dir * i * math.pi / 6
		stg:Wait(3)
	end
end

-- 撃ち返し弾を放つ敵。
-- enemyColor: 敵の色; initPosY: 初期位置; speed: 移動の速さ（正負を考慮）.
local function Revenge(enemyColor, initPosY, speed)
	local initPosX, angle
	if speed < 0 then
		initPosX = ScreenWidth
		angle = math.pi
	else
		initPosX = 0
		angle = 0
	end
	local enemy = stg:CreateEnemy(enemyColor, initPosX, initPosY, math.abs(speed), angle, 8)
	while enemy.HitPoint > 0 do
		coroutine.yield()
	end
	local bulletColor = (enemyColor == EnemyID.SmallBlue) and BulletID.RiceBlue or BulletID.RiceRed
	local max = math.random(10)
	for n = 1, max do
		GenerateBullet(bulletColor, enemy.PosX, enemy.PosY, 4, math.pi * math.random())
		GenerateEffect(EffectID.EnemyShotSound)
	end
end

function stage:Start()
	stg:Wait(120)
	StartCoroutine(Fission, EnemyID.SmallRed, ScreenWidth / 4)
	StartCoroutine(Fission, EnemyID.SmallBlue, ScreenWidth * 3 / 4)
	stg:Wait(320)
	for i = 1, 7 do
		local color, speed
		if i % 2 == 0 then
			color = EnemyID.SmallRed
			speed = 4
		else
			color = EnemyID.SmallBlue
			speed = -4
		end
		for j = 1, 6 do
			StartCoroutine(Revenge, color, i * ScreenWidth / 14, speed)
			stg:Wait(8)
		end
	end
	stg:Wait(300)
end

return stage