NEW ITEMS:

IMPLEMENTED frozen heart: armor, ammo/clip capacity, decreases enemy fire rate in radius around player

IMPLEMENTED rod of ages: increases dmg, health, ammo/clip capacity
	stacks accumulate over time
	at max stacks, player gets a random low tier pickup
	passive effect of dmg taken -> ammo restore
	
runaan's hurricane: fire rate, shoots extra projectiles (boring)

IMPLEMENTED collector: dmg, execute enemies below % health, enemies drop extra casings on death

everfrost: dmg, clip/ammo capacity, health
	active fires cone out in front of player, center freezes enemies, edge chills enemies

IMPLEMENTED galeforce: dmg, fire rate, active dashes player, fires 3 missiles that home on enemies
	no missing health scaling

IMPLEMENTED puppeteer: fire rate, some extra stats?
    passive on-hit: each bullet applies stack to enemy, when max stacks, enemy is charmed and attacks enemies
	when stacks take effect, item goes on cooldown for duration

IMPLEMENTED navori quickblades: fire rate
	passive on-hit: reduce current cooldown of active items by %amount per hit
	(maybe add minimum reduction amount)

IMPLEMENTED rylai's crystal scepter: dmg, health,
	passive on-hit: slows enemies for a short duration

IMPLEMENTED shadowflame: dmg,
	passive: increases dmg against enemies below % health

zeke's convergence: health, armor
	passive: creates aoe around player that slows and deals dmg upon item use

IMPLEMENTED horizon focus: dmg
	passive: dmg increase based on distance from target

maybe add luden's item:
	dmg, ammo/clip capacity
	passive: bullet on hit dmgs target for some damage, goes on cooldown

add league item shop into the game as a custom shopkeeper?
	custom asset building

OPTIMIZATIONS/UPDATES:

IMPLEMENTED optimize kraken slayer level scaling on damage? 
	make it work for all floors and modded floors without explicitly setting the numbers

optimize guinsoo's rageblade phantomHit?
	make it count per volley instead of per bullet?
	make the phantomHit projectile be affected by volley modifiers?

figure out redemption LMAO

maybe change statikk shiv to be time-based cooldown instead of per reload?

maybe make some weapons based on energize items?

maybe add some ui assets for items with cooldowns or stack counts?

potentially make some custom weapons based on league items
	