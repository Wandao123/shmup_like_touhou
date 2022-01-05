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

function TestBenchmark()
	stg:Wait(70)
	local redEnemy = stg:CreateEnemy(EnemyID.SmallRed, ScreenWidth / 3, 0, 1.5, math.pi / 2, 80)
	local blueEnemy = stg:CreateEnemy(EnemyID.SmallBlue, ScreenWidth * 2 / 3, 0, 1.5, math.pi / 2, 80)
	stg:Wait(90)
	redEnemy.Speed = 0
	blueEnemy.Speed = 0
	stg:Wait(5)
	local ways = 51
	local maxIteration = 60
	local diffAngle = 2 * math.pi / ways
	for i = 1, maxIteration do
		local playerDirFromRed = math.pi / 2 - math.atan(GetPlayer().PosX - redEnemy.PosX, GetPlayer().PosY - redEnemy.PosY)
		local playerDirFromBlue = math.pi / 2 - math.atan(GetPlayer().PosX - blueEnemy.PosX, GetPlayer().PosY - blueEnemy.PosY)
		for j = -ways / 3, ways / 3 do
			GenerateBullet(BulletID.SmallRed, redEnemy, 2, playerDirFromRed + j * diffAngle)
			GenerateBullet(BulletID.SmallBlue, blueEnemy, 2, playerDirFromBlue + j * diffAngle)
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
	local playerScript = require('reimu')
	playerScript:Run()
	--StartCoroutine(routine)
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
		coroutine.resume(co, ScreenWidth / 4, 2)
		coroutine.yield()  -- 1フレーム毎に呼び出し元に返す。
	until coroutine.status(co) == 'dead'
	collectgarbage()]]
end
