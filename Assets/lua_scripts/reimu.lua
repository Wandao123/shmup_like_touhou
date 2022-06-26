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
		Vector2.__new(PlayerSize.x, PlayerSize.y / 8),
		Vector2.__new(PlayerSize.x / 2, PlayerSize.y * 5 / 8),
		Vector2.__new(-PlayerSize.x / 2, PlayerSize.y * 5 / 8),
		Vector2.__new(-PlayerSize.x, PlayerSize.y / 8)
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
		player.Position = ScreenBottom - Vector2.__new(0, PlayerSize.y)
		player:Spawned()
		player:TurnInvincible(parameters.InvincibleFrames);
		coroutine.yield()
		stg:Wait(parameters.InputDelayFrames, function()
			-- ここでVelocityを変更すると、移動制限処理のところで不具合が生じる。
			player.Position = player.Position + ScreenTop.normalized
		end)
	end
end

-- 自機の移動。復帰との兼ね合い（復帰中は入力を受け付けない）から、Playerクラス内で処理できない。
local function Move()
	while true do
		player.SlowMode = GetKey(CommandID.Slow)
		-- atan2を使えばもっと短く書けるが、三角関数は計算時間が掛かるため、ループ内では使いたくない。
		if GetKey(CommandID.Rightward) and GetKey(CommandID.Forward) then
			player.Angle = 45
			player.Speed = 1.0
		elseif GetKey(CommandID.Leftward) and GetKey(CommandID.Forward) then
			player.Angle = 135
			player.Speed = 1.0
		elseif GetKey(CommandID.Leftward) and GetKey(CommandID.Backward) then
			player.Angle = 225
			player.Speed = 1.0
		elseif GetKey(CommandID.Rightward) and GetKey(CommandID.Backward) then
			player.Angle = 315
			player.Speed = 1.0
		elseif GetKey(CommandID.Rightward) then
			player.Angle = 0
			player.Speed = 1.0
		elseif GetKey(CommandID.Forward) then
			player.Angle = 90
			player.Speed = 1.0
		elseif GetKey(CommandID.Leftward) then
			player.Angle = 180
			player.Speed = 1.0
		elseif GetKey(CommandID.Backward) then
			player.Angle = 270
			player.Speed = 1.0
		else
			player.Angle = 90
			player.Speed = 0.0
		end
		for i = 1, #options do
			options[i].Position = player.Position + parameters.OptionAlignment[i]
		end
		coroutine.yield()
	end
end

-- 自機のショット。
local function Shoot()  -- 霊夢だと、当たり判定を中心から移動させているため、敵に近づきすぎると当たらなくなるバグあり。
	while true do
		if GetKey(CommandID.Shot) then
			GeneratePlayerBullet(parameters.NormalShot, player.Position.x - 12, player.Position.y, parameters.BulletSpeed, 90)
			GeneratePlayerBullet(parameters.NormalShot, player.Position.x + 12, player.Position.y, parameters.BulletSpeed, 90)
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
		return life
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
		status, life = pcall(detectDown)  -- 被弾した直後に実行したい。
		if not status then
			io.stderr:write('Error: ' + values + '\n')
		end
	until life == 0
end

function script:GetPlayer()
	return player
end

return script