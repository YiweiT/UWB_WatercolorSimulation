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
float eps_b = 0.00005; // Evoparation rate at boundary
float eps = 0.005; // Evoparation rate
float beta = 1; // water capacity of cell
float lambda = 0.1; // range [0.1, 0.6], used in collision step
float theta = 0.1; // min amount of water to flow - used in boundary update step 2
float _recepitivity = 0.3; // recepitivity parameter - used in ws determine in step 1
float _baseMask = 0.5; // base value for ws - step 1




