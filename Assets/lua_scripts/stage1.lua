-- モジュールとして読み込む場合は最初に空のテーブルを作成して、最後にこれをreturnする。
local stage = {}

local stg = require('stg')
local player = nil

-- フレーム数frames毎にenemyオブジェクトから自機狙い弾を生成する。
local function ShootPer(id, frames, enemy)
	local MaxFrames = 120
	local i = 0
	while i <= MaxFrames and enemy:IsEnabled() do
		if i % frames == 0 then
			stg:CreateBullet(id, enemy, 4, stg:CalcAngleBetween(enemy, player))
			--GenerateEffect(EffectID.EnemyShotSound)
		end
		coroutine.yield()
		i = i + 1
	end
end

-- 始めは真っ直ぐに進み、途中で横に曲がる敵。途中で自機狙い弾を発射。
-- id: 敵のID; initPosX: 初期位置; speed: 速さ; radius: 曲がる際に描く円の半径; dir: 曲がる方向（正負）; delay: ショットの間隔.
local function Curve(id, initPosX, speed, radius, dir, delay)
	dir = dir / math.abs(dir)
	local angle = math.pi * 3 / 2
	local enemy = stg:CreateEnemy(id, initPosX, ScreenTop.y, speed, angle, 8)
	coroutine.yield()
	if id == EnemyID.SmallRedFairy then
		StartCoroutine(ShootPer, BulletID.TinyRedBullet, delay, enemy)
	elseif id == EnemyID.SmallBlueFairy then
		StartCoroutine(ShootPer, BulletID.TinyBlueBullet, delay, enemy)
	end
	stg:Wait(90)
	-- 1フレームで進む長さ＝speed。これと円の半径を二辺とする二等辺三角形に対して、余弦定理を適用する。
	local diffAngle = math.acos(1.0 - 0.5 * speed ^ 2 / radius ^ 2)
	angle = angle + dir * diffAngle / 2  -- 曲がり始めはキャラの初期角度も考慮する。
	repeat
		enemy.Angle = angle
		angle = angle + dir * diffAngle  -- 曲がっている途中は正確にこの値だけずれてゆく。
		coroutine.yield()
	until (dir >= 0) and (angle > 2 * math.pi) or (angle < math.pi)  -- 三項演算子
end

-- 真っ直ぐ降りてきて、全方位弾を発射してから引き返す敵。
-- initPosX: 初期位置; ways: 弾数.
local function AllDirection(initPosX, ways)
	local enemyColor = (ways % 2 == 0) and EnemyID.SmallBlueFairy or EnemyID.SmallRedFairy  -- 偶数弾なら青、奇数弾なら赤。
	local bulletColor = (ways % 2 == 0) and BulletID.SmallBlueBullet or BulletID.SmallRedBullet
	local enemy = stg:CreateEnemy(enemyColor, initPosX, ScreenTop.y, 1.5, -math.pi / 2, 24)
	stg:Wait(100)
	enemy.Speed = 0
	stg:Wait(5)
	local diffAngle = 2 * math.pi / ways
	local startingAngle = 0
	if ways % 2 == 0 then
		-- 自機の方向から diffAngle / 2 だけずれた方向。
		startingAngle = stg:CalcAngleBetween(enemy, player) + diffAngle / 2
	else
		-- 自機の方向
		startingAngle = stg:CalcAngleBetween(enemy, player)
	end
	if enemy:IsEnabled() then
		for i = 0, ways - 1 do
			stg:CreateBullet(bulletColor, enemy, 1.5, startingAngle + i * diffAngle)
		end
		--GenerateEffect(EffectID.EnemyShotSound)
	end
	stg:Wait(8)
	if enemy:IsEnabled() then
		for i = 0, ways - 1 do
			stg:CreateBullet(bulletColor, enemy, 1.5, startingAngle + i * diffAngle)
			stg:CreateBullet(bulletColor, enemy, 2.5, startingAngle + i * diffAngle)
		end
		--GenerateEffect(EffectID.EnemyShotSound)
	end
	stg:Wait(15)
	local dir = (initPosX > ScreenCenter.x) and 1 or -1  -- 初期位置が左寄りなら右向きに、右寄りなら左向きに進む。
	enemy.Speed = 3
	for i = 0, 5 do
		enemy.Angle = -math.pi / 2 + dir * i * math.pi / 6
		stg:Wait(3)
	end
end

-- 『東方紅魔郷』1面前半道中の大雑把な再現。ただし、4番目の編隊はランダムなので、原作とは異なる。
function stage:Start()
	player = GetPlayer()
	stg:Wait(120)
	for i = 1, 5 do  -- 5体生成する。
		StartCoroutine(Curve, EnemyID.SmallBlueFairy, stg:DivideInternallyScreenTop(1, 3).x, 2, 30, 1, 30)
		stg:Wait(15)
	end
	stg:Wait(120)
	for i = 1, 5 do
		StartCoroutine(Curve, EnemyID.SmallRedFairy, stg:DivideInternallyScreenTop(3, 1).x, 2, 30, -1, 30)
		stg:Wait(15)
	end
	stg:Wait(120)
	for i = 1, 9 do
		StartCoroutine(Curve, EnemyID.SmallBlueFairy, ScreenTop.x - 20 * (10 - i) , 2, 30, -1, 120)
		StartCoroutine(Curve, EnemyID.SmallBlueFairy, ScreenTop.x + 20 * (10 - i) , 2, 30, 1, 120)
		stg:Wait(15)
	end
	stg:Wait(120)
	for i = 1, 15 do
		StartCoroutine(AllDirection, math.random(stg:DivideInternallyScreenTop(1, 4).x, stg:DivideInternallyScreenTop(4, 1).x), (i % 2 == 0) and (20) or (19))
		stg:Wait(90)
	end
	stg:Wait(10)
	for i = 1, 5 do
		local id = (i % 2 == 1) and EnemyID.SmallBlueFairy or EnemyID.SmallRedFairy
		StartCoroutine(Curve, id, stg:DivideInternallyScreenTop(3, 1).x - 15, 2, 30, -1, 60)
		StartCoroutine(Curve, id, stg:DivideInternallyScreenTop(3, 1).x + 15, 2, 60, -1, 60)
		stg:Wait(15)
	end
	stg:Wait(120)
	for i = 1, 5 do
		local id = (i % 2 == 1) and EnemyID.SmallRedFairy or EnemyID.SmallBlueFairy
		StartCoroutine(Curve, id, stg:DivideInternallyScreenTop(1, 3).x - 15, 2, 60, 1, 60)
		StartCoroutine(Curve, id, stg:DivideInternallyScreenTop(1, 3).x + 15, 2, 30, 1, 60)
		stg:Wait(15)
	end
	stg:Wait(370)
end

return stage
