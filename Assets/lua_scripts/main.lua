-- Unityから呼ばれるのはmain.luaのみとする。このファイルを起点にゲームスクリプトを記述する。

--[[local function foo(initPosX, speed)
	for i = 1, 5 do
		local enemy = GenerateEnemy(EnemyID.SmallBlue, initPosX, 0, speed, math.pi / 2, 8)
		for i = 1, 15 do
			coroutine.yield()
		end
	end
	for i = 1, 180 do
		coroutine.yield()
	end
end]]

local stg = require('stg')
local playerScript = require('reimu')

local function TestBenchmark()
	stg:Wait(70)
	--local redEnemy = stg:CreateEnemy(EnemyID.SmallRedFairy, (ScreenLeft.x - ScreenRight.x) * 1.0 / 3, ScreenTop.y, 1.5, -math.pi / 2, 80)
	--local blueEnemy = stg:CreateEnemy(EnemyID.SmallBlueFairy, (ScreenLeft.x - ScreenRight.x) * 2.0 / 3, ScreenTop.y, 1.5, -math.pi / 2, 80)
	local redEnemy = GenerateEnemy(EnemyID.SmallRedFairy, ScreenLeft.x + (ScreenRight.x - ScreenLeft.x) * 1.0 / 3, ScreenTop.y, 1.5, -math.pi / 2, 80)
	local blueEnemy = GenerateEnemy(EnemyID.SmallBlueFairy, ScreenLeft.x + (ScreenRight.x - ScreenLeft.x) * 2.0 / 3, ScreenTop.y, 1.5, -math.pi / 2, 80)
	stg:Wait(90)
	redEnemy.Speed = 0
	blueEnemy.Speed = 0
	stg:Wait(5)
	local ways = 51
	local maxIteration = 60
	local diffAngle = 2 * math.pi / ways
	for i = 1, maxIteration do
		local playerDirFromRed = math.atan2(playerScript:GetPlayer().PosY - redEnemy.PosY, playerScript:GetPlayer().PosX - redEnemy.PosX)
		local playerDirFromBlue = math.atan2(playerScript:GetPlayer().PosY - blueEnemy.PosY, playerScript:GetPlayer().PosX - blueEnemy.PosX)
		for j = -(ways - 1) / 2, (ways - 1) / 2 do
			if redEnemy:IsEnabled() then
				GenerateBullet(BulletID.SmallRedBullet, redEnemy.PosX, redEnemy.PosY, 2, playerDirFromRed + j * diffAngle)
			end
			if blueEnemy:IsEnabled() then
				GenerateBullet(BulletID.SmallBlueBullet, blueEnemy.PosX, blueEnemy.PosY, 2, playerDirFromBlue + j * diffAngle)
			end
		end
		if redEnemy:IsEnabled() or blueEnemy:IsEnabled() then
			--GenerateEffect(EffectID.EnemyShotSound)
		end
		stg:Wait(1)
	end
	stg:Wait(300)
end

function Main()
	math.randomseed(os.time())
	--StartCoroutine(playerScript.Run)
	local co = { coroutine.create(function() playerScript:Run() end), coroutine.create(TestBenchmark) }
	repeat
		for i = 1, #co do
			if coroutine.status(co[i]) != 'dead' then  -- テーブルから削除する方法？
				coroutine.resume(co[i])
			end
		end
		coroutine.yield()
	until false
	--TestBenchmark()
	--[[local stage1 = require('scripts.stage1')
	stage1:Start()
	ChangeScene(SceneID.StageClear)
	local stage2 = require('scripts.stage2')
	stage2:Start()
	ChangeScene(SceneID.AllClear)]]

	--[[Lua側でコルーチンを実行する場合。
	local co = coroutine.create(foo)
	repeat
		coroutine.resume(co, (ScreenCenter.x - ScreenLeft.x) / 2, 2)
		coroutine.yield()  -- 1フレーム毎に呼び出し元に返す。
	until coroutine.status(co) == 'dead'
	collectgarbage()]]
end
