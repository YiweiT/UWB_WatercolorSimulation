// Weight of distribution function in collision step
float w1 = 4. / 9.; // for stable (f0)
float w2 = 1. / 9.; // for nearest directions (f1 - f4)
float w3 = 1. / 36.; // for diagonal direction (f5 - f8)

// unit direction of distribution functions
float2 e0 = float2(0, 0);
float2 e1 = float2(1, 0);
float2 e2 = float2(0, 1);
float2 e3 = float2(-1, 0);
float2 e4 = float2(0, -1);
float2 e5 = float2(1, 1);
float2 e6 = float2(-1, 1);
float2 e7 = float2(-1, -1);
float2 e8 = float2(1, -1);

// constant
float c_squ = 1; // 3 over c^2 - used in collision to calculate equilibrum function
float eps_b = 0.000001; // Evoparation rate at boundary
float eps = 0.001; // Evoparation rate - rho_update
float beta = 1.05; // water capacity of cell
float lambda = 0.3; // range [0.1, 0.6], used in collision step
float theta = 0.001; // min amount of water to flow - used in boundary update step 2
float _recepitivity = 1; // recepitivity parameter - used in ws determine in step 1
float _baseMask = 1; // base value for ws - step 1
float omega = 1.05; // relaxation rate (determine the viscosity of the fluid, omega < 1 --> more viscous like honey, omega > 1 --> less viscous like water) - collide
float sigma = 0.05; // parameter for blocking the advection when the flow speed is low - pigment advection
float rho_0 = 1;
float drynessPara = 0.1; // modulate P_fix by dryness - used in pigment fixture
float deposite_base = 0.8; // a base rate of deposition - used in pigment fixture
float settlingSpeed = 0.001; // the settling speed of the pigments - used in granularity and back-runs in pigment fixture
float granulThres = 0.015; // a threshold value for which granularity occurs 
float granularity = 0.1; // weight for the strength of the granularity
float re_absorb = 0.05;
float spdMul = 2;

