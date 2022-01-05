local script = {}

local stg = require('stg')

local parameters = {
	InvincibleFrames = 360,
	InputDelayFrames = 90,
	ID = PlayerID.Reimu,
	Option = PlayerID.ReimuOption,
	NormalShot = BulletID.ReimuNormalBullet,
	ShotDelayFrames = 6,
	BulletSpeed = 30.0,
	OptionAlignment = {
		{ PlayerWidth, PlayerHeight / 8 },
		{ PlayerWidth / 2, PlayerHeight * 5 / 8 },
		{ -PlayerWidth / 2, PlayerHeight * 5 / 8 },
		{ -PlayerWidth, PlayerHeight / 8 }
	}
}

local player = nil
local options = {}

-- 初期化。
local function Initialize()
	player = GeneratePlayer(parameters.ID, ScreenWidth * 0.5, ScreenHeight + PlayerHeight - parameters.InputDelayFrames)
	for i = 1, 4 do
		--options[i] = GeneratePlayer(parameters.Option, player.PosX + parameters.OptionAlignment[i][1], player.PosY + parameters.OptionAlignment[i][2])
	end
	player:TurnInvincible(parameters.InvincibleFrames / 2)
	coroutine.yield()
end

-- 自機の復帰処理。
local function Rebirth()
	if (not player:IsEnabled()) and (player.HitPoint > 0) then
		player.PosX = ScreenWidth * 0.5
		player.PosY = ScreenHeight + PlayerHeight
		player:Spawned()
		player:TurnInvincible(parameters.InvincibleFrames);
		coroutine.yield()
		for i = 1, parameters.InputDelayFrames do
			-- ここでSetVelocityを使うと、移動制限処理のところで不具合が生じる。
			player.PosY = player.PosY - 1.0
			coroutine.yield()
		end
	end
end

-- 自機の移動。復帰との兼ね合い（復帰中は入力を受け付けない）から、Playerクラス内で処理できない。
local function Move()
	while true do
		local dirX, dirY = 0.0, 0.0
		if GetKey(CommandID.Leftward) then
			dirX = -1
		end
		if GetKey(CommandID.Rightward) then
			dirX = 1
		end
		if GetKey(CommandID.Forward) then
			dirY = -1
		end
		if GetKey(CommandID.Backward) then
			dirY = 1
		end
		player.SlowMode = GetKey(CommandID.Slow)
		player:SetVelocity(dirX, dirY)
		for i = 1, #options do
			options[i].PosX = player.PosX + parameters.OptionAlignment[i][1]
			options[i].PosY = player.PosY + parameters.OptionAlignment[i][2]
		end
		coroutine.yield()
	end
end

-- 自機のショット。
local function Shoot()
	while true do
		if GetKey(CommandID.Shot) then
			GeneratePlayerBullet(parameters.NormalShot, player.PosX - 12, player.PosY, parameters.BulletSpeed, -math.pi / 2)
			GeneratePlayerBullet(parameters.NormalShot, player.PosX + 12, player.PosY, parameters.BulletSpeed, -math.pi / 2)
			--GenerateEffect(EffectID.PlayerShotSound)
			stg:Wait(parameters.ShotDelayFrames)
		end
		coroutine.yield()
	end
end

-- 自機の被弾処理。
local function Down()
	local life = player.HitPoint
	-- TODO: 被弾するとオプションが暫く残るので、即座に消す方法？
	return function()
		if player.HitPoint < life then
			--GenerateEffect(EffectID.DefetedPlayer, player.PosX, player.PosY)
			life = player.HitPoint
		end
	end
end

function script:Run()
	Initialize()
	local co = { coroutine.create(Shoot), coroutine.create(Move) }
	local detectDown = Down()
	repeat
		local status, values = pcall(Rebirth)
		if not status then
			io.stderr:write('Error: ' + values + '\n')
		end
		for i = 1, #co do
			status, values = coroutine.resume(co[i])
			if not status then
				io.stderr:write('Error: ' + values + '\n')
			end
		end
		coroutine.yield()
		status, values = pcall(detectDown)  -- 被弾した直後に実行したい。
		if not status then
			io.stderr:write('Error: ' + values + '\n')
		end
	until false
end

function script:GetPlayer()
	return player
end

return script