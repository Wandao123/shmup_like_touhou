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
		{ PlayerSize.x, PlayerSize.y / 8 },
		{ PlayerSize.x / 2, PlayerSize.y * 5 / 8 },
		{ -PlayerSize.x / 2, PlayerSize.y * 5 / 8 },
		{ -PlayerSize.x, PlayerSize.y / 8 }
	}
}

local player = nil
local options = {}

-- 初期化。
local function Initialize()
	player = GeneratePlayer(parameters.ID, ScreenCenter.x, ScreenBottom.y - PlayerSize.y + parameters.InputDelayFrames)
	for i = 1, 4 do
		--options[i] = GeneratePlayer(parameters.Option, player.PosX + parameters.OptionAlignment[i][1], player.PosY + parameters.OptionAlignment[i][2])
	end
	player:TurnInvincible(parameters.InvincibleFrames / 2)
	coroutine.yield()
end

-- 自機の復帰処理。
local function Rebirth()
	if (not player:IsEnabled()) and (player.HitPoint > 0) then
		player.PosX = ScreenCenter.x
		player.PosY = ScreenBottom.y - PlayerSize.y
		player:Spawned()
		player:TurnInvincible(parameters.InvincibleFrames);
		coroutine.yield()
		for i = 1, parameters.InputDelayFrames do
			-- ここでSetVelocityを使うと、移動制限処理のところで不具合が生じる。
			player.PosY = player.PosY + ScreenTop.normalized
			coroutine.yield()
		end
	end
end

-- 自機の移動。復帰との兼ね合い（復帰中は入力を受け付けない）から、Playerクラス内で処理できない。
local function Move()
	while true do
		local dirX, dirY = 0.0, 0.0
		if GetKey(CommandID.Leftward) then
			dirX = ScreenLeft.x
		end
		if GetKey(CommandID.Rightward) then
			dirX = ScreenRight.x
		end
		if GetKey(CommandID.Forward) then
			dirY = ScreenTop.y
		end
		if GetKey(CommandID.Backward) then
			dirY = ScreenBottom.y
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
			GeneratePlayerBullet(parameters.NormalShot, player.PosX - 12, player.PosY, parameters.BulletSpeed, math.pi / 2)
			GeneratePlayerBullet(parameters.NormalShot, player.PosX + 12, player.PosY, parameters.BulletSpeed, math.pi / 2)
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