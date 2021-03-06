-- Unityから呼ばれるのはmain.luaのみとする。このファイルを起点にゲームスクリプトを記述する。

--[[local function foo(initPosX, speed)
	for i = 1, 5 do
		local enemy = GenerateEnemy(EnemyID.SmallBlueFairy, initPosX, ScreenTop.y, speed, -90, 8)
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
	local redEnemy = stg:CreateEnemy(EnemyID.SmallRedFairy, ScreenLeft.x + (ScreenRight.x - ScreenLeft.x) * 1.0 / 3, ScreenTop.y, 1.5, -90, 40)
	local blueEnemy = stg:CreateEnemy(EnemyID.SmallBlueFairy, ScreenLeft.x + (ScreenRight.x - ScreenLeft.x) * 2.0 / 3, ScreenTop.y, 1.5, -90, 40)
	stg:Wait(90)
	redEnemy.Speed = 0
	blueEnemy.Speed = 0
	stg:Wait(5)
	local ways = 21
	local maxIteration = 60
	local diffAngle = 360.0 / ways
	for i = 1, maxIteration do
		local playerDirFromRed = stg:CalcAngleBetween(redEnemy, playerScript:GetPlayer())
		local playerDirFromBlue = stg:CalcAngleBetween(blueEnemy, playerScript:GetPlayer())
		for j = -(ways - 1) / 2, (ways - 1) / 2 do
			stg:CreateBullet(BulletID.SmallRedBullet, redEnemy, 2, playerDirFromRed + j * diffAngle)
			stg:CreateBullet(BulletID.SmallBlueBullet, blueEnemy, 2, playerDirFromBlue + j * diffAngle)
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
	StartCoroutine(playerScript.Run)
	--TestBenchmark()
	local stage1 = require('stage1')
	stage1:Start(playerScript:GetPlayer())
	--[[ChangeScene(SceneID.StageClear)
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
